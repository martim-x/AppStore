using System;
using BCrypt.Net;
using Project.Data;

namespace Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository _repo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly SHA256Hasher _sha256Hasher;
        private User? _currentUser;

        public User? CurrentUser => _currentUser;
        public bool IsAdmin => _currentUser?.Role == UserRole.Admin;

        public AuthService(IRepository repo, IPasswordHasher passwordHasher, SHA256Hasher sha256Hasher)
        {
            _repo = repo;
            _passwordHasher = passwordHasher;
            _sha256Hasher = sha256Hasher;
        }

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

        public void UpdateProfile(string? firstName, string? lastName, string? email)
        {
            if (_currentUser == null)
                throw new InvalidOperationException("No user is logged in.");

            _currentUser.FirstName = firstName;
            _currentUser.LastName = lastName;
            _currentUser.Email = email;
            _repo.UpdateUser(_currentUser);
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            if (_currentUser == null)
                throw new InvalidOperationException("No user is logged in.");

            if (!VerifyPassword(oldPassword, _currentUser.PasswordHash))
                throw new UnauthorizedAccessException("Old password is incorrect.");

            _currentUser.PasswordHash = _passwordHasher.HashPassword(newPassword);
            _repo.UpdateUser(_currentUser);
        }

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
