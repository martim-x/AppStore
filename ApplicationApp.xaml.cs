using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Project.Configuration;
using Project.Data;
using Project.Services;
using Project.ViewModels;
using Project.Views;

namespace Project
{
    public partial class ApplicationApp : Application
    {
        private IServiceProvider _serviceProvider;

        [STAThread]
        public static void Main()
        {
            var app = new ApplicationApp();
            app.InitializeComponent();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);

            // Build DI container
            _serviceProvider = CompositionRoot.Build();

            // Register global cursor
            var resourceInfo = Application.GetResourceStream(
                new Uri("Resources/Cursors/arrow.cur", UriKind.Relative));
            if (resourceInfo != null)
            {
                var appCursor = new Cursor(resourceInfo.Stream);
                EventManager.RegisterClassHandler(
                    typeof(Window),
                    Window.LoadedEvent,
                    new RoutedEventHandler(
                        (sender, _) =>
                        {
                            if (sender is Window window)
                                window.Cursor = appCursor;
                        })
                );
            }

            this.ShowLogin();
        }

        private void ShowLogin()
        {
            var repo = _serviceProvider.GetRequiredService<IRepository>();
            var auth = _serviceProvider.GetRequiredService<IAuthService>();
            var login = _serviceProvider.GetRequiredService<Views.LoginWindow>();

            // Inject dependencies into LoginWindow
            login.Initialize(auth);

            // Закрыли логин без входа — завершаем процесс
            if (login.ShowDialog() != true)
            {
                this.Shutdown();
                return;
            }

            this.OpenCatalog();
        }

        private void OpenCatalog()
        {
            var repo = _serviceProvider.GetRequiredService<IRepository>();
            var auth = _serviceProvider.GetRequiredService<IAuthService>();
            var vm = _serviceProvider.GetRequiredService<MainViewModel>();
            var window = _serviceProvider.GetRequiredService<Views.MainWindow>();

            window.DataContext = vm;
            var isLogout = false;

            // Закрываем каталог — завершаем процесс
            window.Closed += (_, __) =>
            {
                if (!isLogout)
                    this.Shutdown();
            };

            // Logout — закрываем каталог и возвращаемся к логину
            vm.LogoutRequested += (_, __) =>
            {
                isLogout = true; // ← до Close(), иначе Closed вызовет Shutdown
                window.Close();
                this.ShowLogin();
            };

            window.Show();
        }
    }
}