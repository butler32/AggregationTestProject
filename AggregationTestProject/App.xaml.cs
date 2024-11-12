using AggregationTestProject.Extensions;
using AggregationTestProject.Services.Api;
using AggregationTestProject.ViewModels;
using log4net;
using System.Windows;
using System.Windows.Threading;
using Unity;

namespace AggregationTestProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILog _log;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                RegisterDependencies();

                Program.Container.RegisterFactory<Dispatcher>(c =>
                {
                    return Dispatcher;
                }, FactoryLifetime.Singleton);

                _log = Program.Container.Resolve<ILog>();

                var appApi = Program.Container.Resolve<ApplicationApiService>();

                await appApi.Login();

                var mainWindow = new MainWindow();

                mainWindow.DataContext = Program.Container.Resolve<MainWindowViewModel>();

                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                _log?.Error("Startup: unknown error", ex);
                MessageBox.Show("Error");
                Current.Shutdown();
            }
        }

        private void RegisterDependencies()
        {
            Program.Container.RegisterServices();
            Program.Container.RegisterOtherServices();
            Program.Container.RegisterViewModels();
            Program.Container.RegisterViews();
        }
    }

}
