using Microsoft.Azure.Relay;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TeklaMcp.Server
{
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            DispatcherUnhandledException += (_, ex) =>
            {
                MessageBox.Show(ex.Exception.Message, "Unhandled Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
            };
        }
    }
}
