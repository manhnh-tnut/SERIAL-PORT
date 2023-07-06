using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Management;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Helpers
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class SerialPortHelper : IDisposable
    {
        public event EventHandler COMPropertyChanged;

        public void OnCOMPropertyChanged([CallerMemberName] string propertyName = null)
        {
            COMPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SerialPortHelper()
        {
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Ports = new ObservableCollection<string>(SerialPort.GetPortNames().OrderBy(s => s));

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

            _watcher = new ManagementEventWatcher(query);
            _watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);
            _watcher.Start();
        }
        private void CheckForNewPorts(EventArrivedEventArgs args)
        {
            // do it async so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(CheckForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public bool ContainPort(string port)
        {
            return Ports.Contains(port);
        }

        private void CheckForNewPortsAsync()
        {
            IEnumerable<string> ports = SerialPort.GetPortNames().Distinct().OrderBy(s => int.Parse(s.Substring(3)));
            if (ports.Count() != Ports.Count)
            {
                Ports = new ObservableCollection<string>(ports);
                OnCOMPropertyChanged();
            }
        }

        public ObservableCollection<string> _Ports = new ObservableCollection<string>();
        public ObservableCollection<string> Ports { get => _Ports; set => _Ports = value; }

        #region IDisposable Members
        public void Dispose()
        {
            _watcher.Stop();
        }
        #endregion

        private ManagementEventWatcher _watcher;
        private TaskScheduler _taskScheduler;
    }
}