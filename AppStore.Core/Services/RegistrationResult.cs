using AppStore.Core.Data;

namespace Project.Services
{
    /// <summary>
    /// Результат регистрации пользователя.
    /// ErrorKey — ключ ресурса локализации с текстом ошибки (RU/EN).
    /// </summary>
    public sealed class RegistrationResult
    {
        private RegistrationResult(bool success, User? user, string? errorKey)
        {
            Success = success;
            User = user;
            ErrorKey = errorKey;
        }

        /// <summary>Признак успешной регистрации.</summary>
        public bool Success { get; }

        /// <summary>Созданный пользователь (при успехе).</summary>
        public User? User { get; }

        /// <summary>Ключ локализации с текстом ошибки (при неуспехе).</summary>
        public string? ErrorKey { get; }

        /// <summary>Успешная регистрация.</summary>
        public static RegistrationResult Ok(User user) => new(true, user, null);

        /// <summary>Ошибка регистрации с ключом локализации.</summary>
        public static RegistrationResult Fail(string errorKey) => new(false, null, errorKey);
    }
}
