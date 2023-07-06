using Extentions;
using Helpers;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO.Ports;
using System.Linq;

namespace Services
{
    public class SerialPortService
    {
        public event EventHandler<string> ScannerDataReceived;
        public event EventHandler<string> ErrorMessage;

        private readonly int baudrate = 9600;
        private readonly string prefix;
        private readonly string suffix;

        private readonly SerialPort GmesPort = new SerialPort();
        private readonly SerialPort ScannerPort = new SerialPort();
        private readonly SerialPortHelper serialPortHelper = new SerialPortHelper();
        private readonly NameValueCollection settings = ConfigurationManager.AppSettings;

        private string buffer;
        public SerialPortService()
        {
            try
            {
                prefix = settings["prefix"];
                suffix = settings["suffix"];
                baudrate = int.Parse(settings["baudrate"]);
            }
            catch { }
            serialPortHelper.COMPropertyChanged += (o, e) =>
            {
                if (serialPortHelper.ContainPort(settings["gmes-port"]))
                {
                    Open(GmesPort, settings["gmes-port"], baudrate);
                }
                else
                {
                    Close(GmesPort);
                }
                if (serialPortHelper.ContainPort(settings["scanner-port"]))
                {
                    Open(ScannerPort, settings["scanner-port"], baudrate);
                }
                else
                {
                    Close(ScannerPort);
                }
            };

            Open(GmesPort, settings["gmes-port"], baudrate);
            Open(ScannerPort, settings["scanner-port"], baudrate);
        }

        private bool Open(SerialPort serialPort, string port, int baud = 9600)
        {
            try
            {
                if (serialPort.IsOpen == true)
                {
                    return true;
                }

                serialPort.PortName = port;
                serialPort.BaudRate = baud;

                serialPort.DataReceived += (o, e) =>
                {
                    try
                    {
                        var data = (o as SerialPort).ReadExisting();
                        buffer += data;
                        if (serialPort.PortName == ScannerPort.PortName && GmesPort?.IsOpen == true)
                        {
                            GmesPort.Write(data);
                        }
                        else
                        {
                            ErrorMessage?.Invoke(null, "Không thể kết nối đến GMES");
                        }
                        if (data.Contains("\r") || data.Contains("\n") || data.Contains("\t"))
                        {
                            if (string.IsNullOrEmpty(data))
                            {
                                return;
                            }
                            ScannerDataReceived?.Invoke(null, buffer.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim());
                            buffer = "";
                        }
                    }
                    catch (Exception error)
                    {
                        ErrorMessage?.Invoke(null, error.Message);
                    }
                };
                serialPort.Open();
                return serialPort.IsOpen;
            }
            catch (Exception e)
            {
                ErrorMessage?.Invoke(null, e.Message);
                return serialPort != null && serialPort.IsOpen;
            }
        }

        private void Close(SerialPort serialPort)
        {
            if (serialPort == null)
            {
                return;
            }
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.Dispose();
        }

        public void SendGmes(string message)
        {
            if (GmesPort?.IsOpen == true)
            {
                var _prefix = Hex2ByteExtention.StringToByteArray(prefix);
                var _message = new System.Text.UTF8Encoding().GetBytes(message);
                var _suffix = Hex2ByteExtention.StringToByteArray(suffix);
                byte[] data = _prefix.Concat(_message).Concat(_suffix).ToArray();
                GmesPort.Write(data, 0, data.Length);
            }
            else
            {
                ErrorMessage?.Invoke(null, "Không thể kết nối đến GMES");
            }
        }

        ~SerialPortService()
        {
            serialPortHelper.Dispose();
            Close(GmesPort);
            Close(ScannerPort);
        }
    }
}
