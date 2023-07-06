using Microsoft.Shell;
using SERIAL_PORT.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SERIAL_PORT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private static readonly string ApplicationID = ResourceAssembly.GetName().Name;
        private static readonly string VSPEconfig = AppContext.BaseDirectory + @"\config.vspe";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(ApplicationID))
            {
                if (System.IO.File.Exists(VSPEconfig) && System.Diagnostics.Process.GetProcessesByName("VSPEmulator").Length == 0)
                {
                    //foreach (var process in System.Diagnostics.Process.GetProcessesByName("VSPEmulator"))
                    //{
                    //    process.Kill();
                    //}
                    System.Diagnostics.Process.Start(VSPEconfig);
                }

                var application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            new ErrorWindow().Show();
            return true;
        }
        #endregion
    }

}
