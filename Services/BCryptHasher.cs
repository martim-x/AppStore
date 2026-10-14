using System;
using BCrypt.Net;

namespace Project.Services
{
    /// <summary>
    /// BCrypt реализация IPasswordHasher. Использует BCrypt.Net-Next.
    /// Work factor настраивается через конфигурацию (по умолчанию 12).
    /// </summary>
    public class BCryptHasher : IPasswordHasher
    {
        private readonly int _workFactor;

        public BCryptHasher(int workFactor = 12)
        {
            _workFactor = workFactor;
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}