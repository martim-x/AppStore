using System;
using System.IO;

namespace AppStore.Core.Configuration
{
    /// <summary>
    /// Централизованный класс настроек приложения.
    /// Все hardcoded значения вынесены сюда. Значения загружаются из .env / appsettings.json / переменных окружения.
    /// </summary>
    public sealed class Settings
    {
        // ── Singleton ────────────────────────────────────────────────────────
        private static readonly Lazy<Settings> _instance = new(() => new Settings());
        public static Settings Instance => _instance.Value;

        private Settings() { Load(); }

        // ── Database ─────────────────────────────────────────────────────────
        public string DbHost { get; private set; } = "localhost";
        public int DbPort { get; private set; } = 5433;
        public string DbName { get; private set; } = "appstore";
        public string DbUser { get; private set; } = "postgres";
        public string DbPassword { get; private set; } = "111";

        /// <summary>Строка подключения для Npgsql / EF Core.</summary>
        public string ConnectionString =>
            $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword}";

        // ── Application ──────────────────────────────────────────────────────
        public string Environment { get; private set; } = "Development";
        public string AppName { get; } = "AppStore";
        public string AppVersion { get; } = "1.0.0";

        // ── Security ─────────────────────────────────────────────────────────
        /// <summary>Алгоритм хеширования паролей. Сейчас SHA-256 (учебный), для продакшена — BCrypt/Argon2.</summary>
        public string PasswordHashAlgorithm { get; } = "SHA256";

        // ── UI / UX ──────────────────────────────────────────────────────────
        public string DefaultLanguage { get; private set; } = "ru";
        public string DefaultTheme { get; private set; } = "Light";
        public int DefaultPageSize { get; private set; } = 6;
        public int MaxPriceFilter { get; private set; } = 1000;

        // ── Docker / Compose (для справки, не используется в runtime) ────────
        public string DockerImage { get; private set; } = "postgres";
        public string DockerTag { get; private set; } = "16-alpine";
        public string DockerVolume { get; private set; } = "appstore_pgdata:/var/lib/postgresql/data";

        // ── Loading ──────────────────────────────────────────────────────────
        private void Load()
        {
            // 1. .env file (highest priority for local dev)
            LoadFromEnvFile();

            // 2. Environment variables (override .env)
            LoadFromEnvironment();

            // 3. appsettings.json (optional, for structured config)
            // LoadFromAppSettings(); // можно добавить при необходимости
        }

        private void LoadFromEnvFile()
        {
            var envPath = FindEnvFile();
            if (envPath == null || !File.Exists(envPath))
                return;

            foreach (var raw in File.ReadAllLines(envPath))
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith('#') || !line.Contains('='))
                    continue;

                var eq = line.IndexOf('=');
                var key = line[..eq].Trim();
                var val = line[(eq + 1)..].Trim().Trim('"');

                SetProperty(key, val);
            }
        }

        private string? FindEnvFile()
        {
            // Приоритет: рядом с исполняемым файлом → CurrentDirectory → корень проекта
            var baseDir = AppContext.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, ".env"),
                Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.env")), // из bin/Debug/net8.0-windows
            };

            foreach (var c in candidates)
                if (File.Exists(c)) return c;

            return null;
        }

        private void LoadFromEnvironment()
        {
            // Переменные окружения перекрывают .env
            SetProperty("DB_HOST", System.Environment.GetEnvironmentVariable("DB_HOST"));
            SetProperty("DB_PORT_FROM", System.Environment.GetEnvironmentVariable("DB_PORT_FROM"));
            SetProperty("DB_NAME", System.Environment.GetEnvironmentVariable("DB_NAME"));
            SetProperty("DB_ADMIN", System.Environment.GetEnvironmentVariable("DB_ADMIN"));
            SetProperty("DB_ADMIN_PASS", System.Environment.GetEnvironmentVariable("DB_ADMIN_PASS"));
            SetProperty("APP_ENV", System.Environment.GetEnvironmentVariable("APP_ENV"));
            SetProperty("DEFAULT_LANGUAGE", System.Environment.GetEnvironmentVariable("DEFAULT_LANGUAGE"));
            SetProperty("DEFAULT_THEME", System.Environment.GetEnvironmentVariable("DEFAULT_THEME"));
        }

        private void SetProperty(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            switch (key)
            {
                case "DB_HOST": DbHost = value; break;
                case "DB_PORT_FROM":
                    if (int.TryParse(value, out var p) && p > 0)
                        DbPort = p;
                    break;
                case "DB_NAME": DbName = value; break;
                case "DB_ADMIN": DbUser = value; break;
                case "DB_ADMIN_PASS": DbPassword = value; break;
                case "APP_ENV": Environment = value; break;
                case "DEFAULT_LANGUAGE": DefaultLanguage = value; break;
                case "DEFAULT_THEME": DefaultTheme = value; break;
                case "DOCKER_IMAGE": DockerImage = value; break;
                case "DOCKER_TAG": DockerTag = value; break;
                case "DOCKER_VOLUME": DockerVolume = value; break;
            }
        }

        // ── Validation ───────────────────────────────────────────────────────
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DbHost)) throw new InvalidOperationException("DB_HOST not configured");
            if (DbPort <= 0 || DbPort > 65535) throw new InvalidOperationException("Invalid DB_PORT");
            if (string.IsNullOrWhiteSpace(DbName)) throw new InvalidOperationException("DB_NAME not configured");
            if (string.IsNullOrWhiteSpace(DbUser)) throw new InvalidOperationException("DB_USER not configured");
            if (string.IsNullOrWhiteSpace(DbPassword)) throw new InvalidOperationException("DB_PASSWORD not configured");
        }

        // ── Debug / Display ──────────────────────────────────────────────────
        public override string ToString()
        {
            return $"{AppName} v{AppVersion} | Env: {Environment} | DB: {DbHost}:{DbPort}/{DbName} | Lang: {DefaultLanguage} | Theme: {DefaultTheme}";
        }
    }
}