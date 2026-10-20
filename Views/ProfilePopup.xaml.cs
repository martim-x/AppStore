using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using AppStore.Core.Data;
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

            Owner = owner;
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

            var vm = new ProfileViewModel(auth, theme);
            vm.GetOldPassword = () => OldPasswordBox.Password;
            vm.GetNewPassword = () => NewPasswordBox.Password;
            DataContext = vm;

            PositionNearOwner();
        }

        private void PositionNearOwner()
        {
            if (Owner == null)
                return;
            Left = Owner.Left + 12;
            Top = Owner.Top + Owner.ActualHeight - 580;
            if (Top < Owner.Top)
                Top = Owner.Top + 20;
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            LoggedOut = true;
            var auth = _services?.GetRequiredService<IAuthService>();
            auth?.Logout();
            Close();
        }

        private void OnDeactivated(object sender, System.EventArgs e) { }

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
