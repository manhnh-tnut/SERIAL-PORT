using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Extensions;
using MahApps.Metro.Controls.Dialogs;
using SERIAL_PORT.Models;
using Serilog;

namespace SERIAL_PORT.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private readonly Views.MainWindow window = null;
        private readonly NameValueCollection settings = ConfigurationManager.AppSettings;
        private Services.SerialPortService serialPortService = null;
        private User _User = null;
        private string _Barcode = "", _Result = "", _Version = "";
        private Brush _Status = Brushes.Transparent;
        public Brush Status { get => _Status; set { _Status = value; OnPropertyChanged(); } }

        public string User { get => _User == null ? "" : "Xin chào " + _User.fullName; set { OnPropertyChanged(); } }
        public string Barcode { get => _Barcode; set { _Barcode = value; OnPropertyChanged(); } }
        public string Result { get => _Result; set { _Result = value; OnPropertyChanged(); } }
        public string Version { get => _Version; set { _Version = value; OnPropertyChanged(); } }
        public ICommand OnBarcodeInputed { get; set; }

        private async Task<User> ShowLoginDialogAsync()
        {
            User user = null;
            window.MetroDialogOptions.ColorScheme = /*MetroDialogColorScheme.Accented: */MetroDialogColorScheme.Theme;
            LoginDialogData result = await window.ShowLoginAsync("Đăng nhập", "", new LoginDialogSettings { ColorScheme = window.MetroDialogOptions.ColorScheme, UsernameWatermark = "Gen", PasswordWatermark = "Mật khẩu" });
            if (result != null)
            {
                if (string.IsNullOrEmpty(result.Username) || string.IsNullOrEmpty(result.Password))
                {
                    await window.ShowMessageAsync("Lỗi", "Vui lòng nhập đủ Gen và mật khẩu.");
                    return user;
                }

                using (EntityModel context = new EntityModel())
                {
                    user = await context.Database.SqlQuery<User>($"EXEC SP_LOGIN {result.Username},{result.Password}").FirstOrDefaultAsync();
                    if (user == null)
                    {
                        await window.ShowMessageAsync("Lỗi", "Vui lòng điền đúng Gen và mật khẩu.");
                    }
                }
            }
            return user;
        }

        private async Task Processing()
        {
            Log.Logger.Information($"Length: {Barcode.Length}\t Value: {Barcode}");
            using (EntityModel context = new EntityModel())
            {
                var rework = await context.Reworks.Where(i => i.barcode == Barcode && i.group == _User.group).FirstOrDefaultAsync();
                if (rework == null)
                {
                    Status = Brushes.GreenYellow;
                    Result = "";
                }
                else
                {
                    Status = Brushes.Red;
                    Result = rework.error.Trim();
                }
            }

            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(1000);
                Barcode = "";
            }, TaskCreationOptions.RunContinuationsAsynchronously);

            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(3000);
                Status = Brushes.Transparent;
                Result = "";
            }, TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public MainViewModel(Views.MainWindow window)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(System.AppContext.BaseDirectory + @"\Logs\log.txt", shared: true, retainedFileCountLimit: 7)
                .CreateLogger();

            this.window = window;
            Version = $"Version: {settings["version"]}";
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(async delegate
                {
                    while (_User == null)
                    {
                        try
                        {
                            _User = await ShowLoginDialogAsync();
                            User = "";
                        }
                        catch { }
                    }

                    serialPortService = new Services.SerialPortService();
                    serialPortService.ScannerDataReceived += async (o, e) =>
                    {
                        Barcode = e;
                        if (_User == null || string.IsNullOrEmpty(Barcode))
                        {
                            return;
                        }
                        await Processing();
                    };

                    serialPortService.ErrorMessage += async (o, e) =>
                    {
                        await Task.Factory.StartNew(() =>
                         {
                             Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(async delegate
                             {
                                 await window.ShowMessageAsync("Lỗi", e);
                             }));
                         }, TaskCreationOptions.RunContinuationsAsynchronously);
                    };

                }));
            }, TaskCreationOptions.RunContinuationsAsynchronously);

            OnBarcodeInputed = new DelegateCommand<object>(async _ =>
            {
                if (string.IsNullOrEmpty(Barcode.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim()))
                {
                    return;
                }
                serialPortService.SendGmes(Barcode);
                await Processing();
            },
            _ =>
            {
                //can this command execute? return true or false
                return true;
            })
            .ListenOn(this, i => i.Barcode, dispatcher);
        }
    }
}
