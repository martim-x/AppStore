using System;

namespace Project.Services
{
    /// <summary>
    /// Абстракция для хеширования паролей. Позволяет подменять алгоритм (SHA-256, BCrypt, Argon2).
    /// </summary>
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}