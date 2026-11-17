using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using AppStore.Core.Data;
using AppStore.Core.Services;
using Project.Services;
using Project.ViewModels;

namespace Project.Views
{
    public partial class ProfilePopup : Window
    {
        private readonly IServiceProvider _services;
        public bool LoggedOut { get; private set; }

        public ProfilePopup(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            InitializeComponent();
        }

        public void Initialize(Window owner)
        {
            var auth = _services.GetRequiredService<IAuthService>();
            var theme = _services.GetRequiredService<IThemeService>();
            var health = _services.GetService<IHealthCheckService>();

            Owner = owner;
            MaxHeight = SystemParameters.WorkArea.Height - 32;
            var user = auth.CurrentUser!;
            AvatarLetter.Text = user.AvatarLetter;
            LoginText.Text = user.Login;

            AvatarBorder.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(user.AvatarColor));

            if (user.Role == UserRole.Admin)
            {
                RoleText.Text = "Admin";
                RoleBadge.Background = new SolidColorBrush(Color.FromRgb(255, 59, 48));
            }
            else
            {
                RoleText.Text = "User";
                RoleBadge.Background = new SolidColorBrush(Color.FromRgb(0, 114, 247));
            }

            var vm = new ProfileViewModel(auth, theme, health);
            vm.GetOldPassword = () => OldPasswordBox.Password;
            vm.GetNewPassword = () => NewPasswordBox.Password;
            DataContext = vm;

            // Позиционирование: WindowStartupLocation=CenterOwner в XAML —
            // окно настроек всплывает по центру главного окна
            // (решение автора 15.11.2026, REPORT-010).
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            LoggedOut = true;
            var auth = _services?.GetRequiredService<IAuthService>();
            auth?.Logout();
            Close();
        }

        private void OnDeactivated(object sender, System.EventArgs e) { }

        /// <summary>
        /// Переключение языка из настроек профиля (APP-001).
        /// Команда живёт в MainViewModel — она же обновляет валюту и сохраняет выбор в профиль.
        /// </summary>
        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.Tag is not string lang)
                return;

            if (Owner is MainWindow main && main.DataContext is MainViewModel vm)
                vm.SwitchLanguageCommand.Execute(lang);
        }

        private void OnPasswordBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender == OldPasswordBox && OldPwBorder != null)
                OldPwBorder.BorderBrush = (Brush)FindResource("AccentBlueBrush");
            if (sender == NewPasswordBox && NewPwBorder != null)
                NewPwBorder.BorderBrush = (Brush)FindResource("AccentBlueBrush");
        }

        private void OnPasswordBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender == OldPasswordBox && OldPwBorder != null)
                OldPwBorder.BorderBrush = (Brush)FindResource("DividerBrush");
            if (sender == NewPasswordBox && NewPwBorder != null)
                NewPwBorder.BorderBrush = (Brush)FindResource("DividerBrush");
        }
    }
}
