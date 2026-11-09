using AppStore.Core.Data;

namespace Project.Services
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        bool IsAdmin { get; }

        User? Login(string login, string password);

        /// <summary>Регистрация обычного пользователя (роль User).</summary>
        RegistrationResult Register(string login, string password);

        void Logout();
        void UpdateProfile(string? firstName, string? lastName, string? email);

        /// <summary>Сохранить настройки интерфейса пользователя.</summary>
        void UpdatePreferences(string language, string theme);
        void ChangePassword(string oldPassword, string newPassword);
    }
}
