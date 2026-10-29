using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AppStore.Core.Services;
using Project.Commands;
using Project.Services;

namespace Project.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IAuthService _auth;
        private readonly IThemeService _theme;
        private readonly IHealthCheckService? _health;

        private string? _firstName;
        private string? _lastName;
        private string? _email;
        private bool _isDarkMode;
        private string _statusMessage = string.Empty;
        private string _databaseStatus = string.Empty;
        private bool _isDatabaseHealthy;

        public string? FirstName
        {
            get => _firstName;
            set => SetField(ref _firstName, value);
        }
        public string? LastName
        {
            get => _lastName;
            set => SetField(ref _lastName, value);
        }
        public string? Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set => SetField(ref _isDarkMode, value);
        }
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        /// <summary>Текст состояния соединения с базой данных.</summary>
        public string DatabaseStatus
        {
            get => _databaseStatus;
            set => SetField(ref _databaseStatus, value);
        }

        /// <summary>Признак успешного состояния БД — для подсветки индикатора.</summary>
        public bool IsDatabaseHealthy
        {
            get => _isDatabaseHealthy;
            set => SetField(ref _isDatabaseHealthy, value);
        }

        public ICommand CheckDatabaseCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        // Delegates for password retrieval from code-behind (PasswordBox can't bind)
        public Func<string>? GetOldPassword { get; set; }
        public Func<string>? GetNewPassword { get; set; }

        public ProfileViewModel(IAuthService auth, IThemeService theme,
                                IHealthCheckService? health = null)
        {
            _auth = auth;
            _theme = theme;
            _health = health;
            _isDarkMode = theme.IsDark;

            var user = auth.CurrentUser;
            if (user != null)
            {
                _firstName = user.FirstName;
                _lastName = user.LastName;
                _email = user.Email;
            }

            SaveProfileCommand = new RelayCommand(SaveProfile);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            CheckDatabaseCommand = new AsyncRelayCommand(CheckDatabaseAsync);

            // Первичная проверка состояния БД при открытии профиля
            _ = CheckDatabaseAsync();
        }

        /// <summary>
        /// Проверяет доступность базы данных и обновляет индикатор состояния.
        /// </summary>
        private async Task CheckDatabaseAsync()
        {
            if (_health == null)
            {
                DatabaseStatus = string.Empty;
                IsDatabaseHealthy = false;
                return;
            }

            var result = await _health.CheckDatabaseAsync().ConfigureAwait(true);
            IsDatabaseHealthy = result.IsHealthy;
            DatabaseStatus = result.IsHealthy
                ? $"БД: {result.Message} ({result.LatencyMs} мс)"
                : $"БД недоступна: {result.Message}";
        }

        private void SaveProfile()
        {
            try
            {
                _auth.UpdateProfile(FirstName, LastName, Email);
                StatusMessage = "Profile saved";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void ChangePassword()
        {
            var oldPw = GetOldPassword?.Invoke() ?? "";
            var newPw = GetNewPassword?.Invoke() ?? "";

            if (string.IsNullOrEmpty(newPw))
            {
                StatusMessage = "New password cannot be empty";
                return;
            }

            try
            {
                _auth.ChangePassword(oldPw, newPw);
                StatusMessage = "Password changed";
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = "Old password is incorrect";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void ToggleTheme()
        {
            _theme.SetTheme(IsDarkMode);

            try
            {
                var user = _auth.CurrentUser;
                if (user != null)
                    _auth.UpdatePreferences(user.Language, IsDarkMode ? "dark" : "light");
            }
            catch
            {
                // профиль ещё не сохранён — настройка применится в следующий раз
            }
        }
    }
}
