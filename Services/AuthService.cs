using System;
using BCrypt.Net;
using Project.Data;

namespace Project.Services
{
    /// <summary>
    /// Сервис аутентификации и авторизации пользователей.
    /// Поддерживает BCrypt для новых паролей и SHA-256 для обратной совместимости.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IRepository _repo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly SHA256Hasher _sha256Hasher;
        private User? _currentUser;

        /// <summary>Текущий авторизованный пользователь.</summary>
                public User? CurrentUser => _currentUser;

                /// <summary>Флаг: текущий пользователь — администратор.</summary>
                public bool IsAdmin => _currentUser?.Role == UserRole.Admin;

                /// <summary>
                /// Конструктор с внедрением зависимостей.
                /// </summary>
                /// <param name="repo">Репозиторий пользователей.</param>
                /// <param name="passwordHasher">Хешер паролей (BCrypt).</param>
                /// <param name="sha256Hasher">Fallback-хешер (SHA-256 для legacy-паролей).</param>
                public AuthService(IRepository repo, IPasswordHasher passwordHasher, SHA256Hasher sha256Hasher)
        {
            _repo = repo;
            _passwordHasher = passwordHasher;
            _sha256Hasher = sha256Hasher;
        }

        /// <summary>
        /// Аутентификация пользователя по логину и паролю.
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <param name="password">Пароль в открытом виде.</param>
        /// <returns>Пользователь при успехе, null — при неудаче.</returns>
        public User? Login(string login, string password)
        {
            var user = _repo.GetUserByLogin(login);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            _currentUser = user;

            if (_repo is JsonRep jsonRep)
                jsonRep.SetCurrentUser(user.Id);

            if (_repo is PostgreSQLRepADO adoRep)
                adoRep.SetCurrentUser(user.Id);

            if (_repo is PostgreSQLRepEF efRep)
                efRep.SetCurrentUser(user.Id);

            return _currentUser;
        }

        /// <summary>
        /// Выход пользователя (очистка сессии).
        /// </summary>
        public void Logout()
        {
            _currentUser = null;

            if (_repo is JsonRep jsonRep)
                jsonRep.SetCurrentUser(null);

            if (_repo is PostgreSQLRepADO adoRep)
                adoRep.SetCurrentUser(null);

            if (_repo is PostgreSQLRepEF efRep)
                efRep.SetCurrentUser(null);
        }

        /// <summary>
        /// Обновление профиля текущего пользователя.
        /// </summary>
        /// <param name="firstName">Имя.</param>
        /// <param name="lastName">Фамилия.</param>
        /// <param name="email">Email.</param>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован.</exception>
        public void UpdateProfile(string? firstName, string? lastName, string? email)
        {
            if (_currentUser == null)
                throw new InvalidOperationException("No user is logged in.");

            _currentUser.FirstName = firstName;
            _currentUser.LastName = lastName;
            _currentUser.Email = email;
            _repo.UpdateUser(_currentUser);
        }

        /// <summary>
        /// Смена пароля текущего пользователя.
        /// </summary>
        /// <param name="oldPassword">Текущий пароль.</param>
        /// <param name="newPassword">Новый пароль.</param>
        /// <exception cref="InvalidOperationException">Пользователь не авторизован.</exception>
        /// <exception cref="UnauthorizedAccessException">Неверный старый пароль.</exception>
        public void ChangePassword(string oldPassword, string newPassword)
        {
            if (_currentUser == null)
                throw new InvalidOperationException("No user is logged in.");

            if (!VerifyPassword(oldPassword, _currentUser.PasswordHash))
                throw new UnauthorizedAccessException("Old password is incorrect.");

            _currentUser.PasswordHash = _passwordHasher.HashPassword(newPassword);
            _repo.UpdateUser(_currentUser);
        }

        /// <summary>
        /// Проверка пароля: сначала BCrypt, затем fallback на SHA-256.
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            // Try BCrypt first (new passwords)
            try
            {
                if (BCrypt.Net.BCrypt.Verify(password, hash))
                    return true;
            }
            catch { }

            // Fallback to SHA-256 (legacy passwords)
            return _sha256Hasher.VerifyPassword(password, hash);
        }

        // Kept for backward compatibility (tests, seed data)
        public static string HashPasswordStatic(string password) =>
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password))).ToLower();
    }
}
