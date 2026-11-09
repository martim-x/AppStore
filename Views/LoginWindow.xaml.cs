using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Project.Services;

namespace Project.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _auth;
        private bool _registerMode;

        public LoginWindow(IAuthService auth)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            InitializeComponent();
            LoginBox.Focus();
        }

        private void OnSignIn(object sender, RoutedEventArgs e) => TryLogin();

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TryLogin();
        }

        private void TryLogin()
        {
            var login = LoginBox.Text.Trim();
            var password = PassBox.Password;

            if (_registerMode)
            {
                TryRegister(login, password);
                return;
            }

            var user = _auth.Login(login, password);

            if (user != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("LoginError");
            }
        }

        /// <summary>
        /// Самостоятельная регистрация обычного пользователя (APP-002).
        /// При успехе сессия открывается сразу, окно закрывается с DialogResult = true.
        /// </summary>
        private void TryRegister(string login, string password)
        {
            if (password != ConfirmBox.Password)
            {
                ShowError("RegisterErrorPasswordMismatch");
                return;
            }

            var result = _auth.Register(login, password);

            if (result.Success)
            {
                DialogResult = true;
                Close();
                return;
            }

            ShowError(result.ErrorKey ?? "LoginError");
        }

        /// <summary>Показать локализованное сообщение об ошибке и подсветить поля.</summary>
        private void ShowError(string resourceKey)
        {
            ErrorText.Text = Localized(resourceKey);
            ErrorBorder.Visibility = Visibility.Visible;

            PassBox.Clear();
            ConfirmBox.Clear();

            var errorBrush = new SolidColorBrush(Color.FromRgb(255, 59, 48));
            LoginFieldBorder.BorderBrush = errorBrush;
            PassFieldBorder.BorderBrush = errorBrush;
            ConfirmFieldBorder.BorderBrush = errorBrush;
        }

        private static string Localized(string resourceKey) =>
            Application.Current.Resources[resourceKey] as string ?? resourceKey;

        private void OnToggleMode(object sender, RoutedEventArgs e)
        {
            _registerMode = !_registerMode;
            ApplyMode();
        }

        /// <summary>Переключить окно между входом и регистрацией.</summary>
        private void ApplyMode()
        {
            SubtitleText.Text = Localized(_registerMode ? "RegisterTitle" : "LoginSubtitle");
            SubmitText.Text = Localized(_registerMode ? "Register" : "SignIn");
            ToggleModeText.Text = Localized(_registerMode ? "HaveAccount" : "NoAccount");

            ConfirmPanel.Visibility = _registerMode ? Visibility.Visible : Visibility.Collapsed;
            LoginHintText.Visibility = _registerMode ? Visibility.Collapsed : Visibility.Visible;

            ErrorBorder.Visibility = Visibility.Collapsed;
            PassBox.Clear();
            ConfirmBox.Clear();

            PassBox.Focus();
        }

        private void OnFieldFocus(object sender, RoutedEventArgs e)
        {
            // Reset red borders on focus
            LoginFieldBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 114, 247));
            PassFieldBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 114, 247));
            ErrorBorder.Visibility = Visibility.Collapsed;
        }

        private void OnFieldBlur(object sender, RoutedEventArgs e)
        {
            LoginFieldBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            PassFieldBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
        }
    }
}
