using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using AppStore.Core.Data;
using AppStore.Core.Configuration;
using Project.Services;
using Xunit;
using FluentAssertions;

namespace AppStore.IntegrationTests
{
    public class DatabaseIntegrationTests : IAsyncLifetime
    {
        private readonly IServiceProvider _services;
        private PostgreSQLRepEF _repository;
        private IAuthService _authService;
        private PostgreDbContext _dbContext;

        public DatabaseIntegrationTests()
        {
            // Локальные значения по умолчанию. В CI они перекрываются
            // переменными окружения от service container
            // (DB_HOST=postgres, DB_PORT_FROM=5432, DB_NAME=appstore_test,
            //  DB_ADMIN=postgres, DB_ADMIN_PASS=postgres) — тесты используют
            // ровно те ключи, поэтому здесь проверяется «уже задано?».
            // Файл .env из корня репозитория тестовому проекту недоступен:
            // он ищется относительно bin-папки проекта, а не репозитория.
            var defaults = new Dictionary<string, string>
            {
                ["DB_HOST"] = "localhost",
                ["DB_PORT_FROM"] = "5433",
                // Отдельная БД: тест RestoreDefaults удаляет всех пользователей,
                // и при работе по рабочей базе стирает демо-учётки и оценки.
                ["DB_NAME"] = "appstore_test",
                ["DB_ADMIN"] = "postgres",
                ["DB_ADMIN_PASS"] = "changeme"
            };

            foreach (var kvp in defaults)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(kvp.Key)))
                    Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }

            // Environment variables are set by CI service container
            // DB_HOST=postgres, DB_PORT_FROM=5432, DB_NAME=appstore_test, etc.

            // Force Settings to reload from environment
            var loadMethod = typeof(Settings).GetMethod("LoadFromEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            loadMethod?.Invoke(Settings.Instance, null);

            // Тестовая БД создаётся автоматически: работа идёт в отдельной базе,
            // чтобы RestoreDefaults не стирал демо-данные рабочего стенда.
            EnsureTestDatabaseExists();

            // Build DI container for core services only
            var services = new ServiceCollection();
            services.AddSingleton(_ => Settings.Instance);
            services.AddSingleton<IPasswordHasher, BCryptHasher>();
            services.AddSingleton<SHA256Hasher>();
            services.AddScoped<IRepository, PostgreSQLRepEF>();
            services.AddScoped<PostgreSQLRepEF>();
            services.AddSingleton<IUndoRedoService, UndoRedoService>();
            services.AddScoped<IAuthService, AuthService>();

            _services = services.BuildServiceProvider();

            var connectionString = Settings.Instance.ConnectionString;

            var options = new DbContextOptionsBuilder<PostgreDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            _dbContext = new PostgreDbContext(options);
            _repository = _services.GetRequiredService<PostgreSQLRepEF>();

            // Replace the internal DbContext
            var repoField = typeof(PostgreSQLRepEF).GetField("_db", BindingFlags.NonPublic | BindingFlags.Instance);
            repoField?.SetValue(_repository, _dbContext);

            _authService = new AuthService(_repository, new BCryptHasher(), new SHA256Hasher());
        }

        public async Task InitializeAsync()
        {
            // Ensure database is created and migrations applied
            await _dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContext.DisposeAsync();
        }

        /// <summary>
        /// Создать тестовую БД, если её ещё нет: миграции применяются позже,
        /// а подключение к несуществующей базе падает на открытии соединения.
        /// </summary>
        private static void EnsureTestDatabaseExists()
        {
            var settings = Settings.Instance;
            var adminBuilder = new NpgsqlConnectionStringBuilder(settings.ConnectionString)
            {
                Database = "postgres"
            };

            using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
            connection.Open();

            using (var exists = new NpgsqlCommand(
                "select 1 from pg_database where datname = @name", connection))
            {
                exists.Parameters.AddWithValue("name", settings.DbName);
                if (exists.ExecuteScalar() != null)
                    return;
            }

            using var create = new NpgsqlCommand(
                $"create database \"{settings.DbName}\"", connection);
            create.ExecuteNonQuery();
        }

        [Fact]
        public void Database_CanConnectAndSeed()
        {
            var users = _repository.GetAllUsers();
            users.Should().NotBeEmpty();
            users.Should().Contain(u => u.Login == "admin");
            users.Should().Contain(u => u.Login == "user");
        }

        [Fact]
        public void Apps_AreSeeded()
        {
            var apps = _repository.GetAllApps();
            apps.Should().NotBeEmpty();
            apps.Should().HaveCountGreaterOrEqualTo(10);
        }

        [Fact]
        public void AuthService_CanLoginAdmin()
        {
            // Seed password for admin is "admin"
            var user = _authService.Login("admin", "admin");
            user.Should().NotBeNull();
            user!.Login.Should().Be("admin");
            _authService.IsAdmin.Should().BeTrue();
        }

        [Fact]
        public void AuthService_CanLoginUser()
        {
            // Seed password for user is "1234"
            var user = _authService.Login("user", "1234");
            user.Should().NotBeNull();
            user!.Login.Should().Be("user");
            _authService.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public void AuthService_InvalidCredentials_ReturnsNull()
        {
            var user = _authService.Login("admin", "wrongpassword");
            user.Should().BeNull();
        }

        [Fact]
        public void Repository_CanAddAndRetrieveApp()
        {
            var newApp = new App
            {
                ShortName = "TestApp",
                FullName = "Test Application",
                Description = "A test app",
                Developer = "TestDev",
                Category = "Testing",
                Rating = 4.5,
                RatingCount = 100,
                Price = 0,
                Version = "1.0.0",
                SizeMB = 50,
                Country = "US",
                AgeRating = "4+",
                Color = "#00FF00",
                IsFeatured = false,
                IsInStock = true,
                DiscountPercent = null,
                ReleaseDate = DateTime.Now,
                Tags = new List<string> { "test", "integration" }
            };

            _repository.AddApp(newApp);

            try
            {
                var retrieved = _repository.GetAppById(newApp.Id);
                retrieved.Should().NotBeNull();
                retrieved!.ShortName.Should().Be("TestApp");
                retrieved.FullName.Should().Be("Test Application");
            }
            finally
            {
                // Тестовое приложение не должно оставаться в каталоге.
                _repository.DeleteApp(newApp.Id);
            }
        }

        [Fact]
        public void Repository_CanUpdateApp()
        {
            var apps = _repository.GetAllApps();
            var app = apps.First();
            var originalName = app.FullName;
            app.FullName = "Updated Name";

            try
            {
                _repository.UpdateApp(app);

                var retrieved = _repository.GetAppById(app.Id);
                retrieved.Should().NotBeNull();
                retrieved!.FullName.Should().Be("Updated Name");
                retrieved.FullName.Should().NotBe(originalName);
            }
            finally
            {
                // Возвращаем каталог в исходное состояние: тест не должен
                // оставлять переименованное приложение в рабочей БД.
                app.FullName = originalName;
                _repository.UpdateApp(app);
            }
        }

        [Fact]
        public void Repository_CanDeleteApp()
        {
            var newApp = new App
            {
                ShortName = "ToDelete",
                FullName = "To Delete App",
                Description = "Will be deleted",
                Developer = "TestDev",
                Category = "Testing",
                Rating = 4.0,
                RatingCount = 10,
                Price = 10,
                Version = "1.0",
                SizeMB = 10,
                Country = "US",
                AgeRating = "4+",
                Color = "#FF0000",
                IsFeatured = false,
                IsInStock = true,
                DiscountPercent = null,
                ReleaseDate = DateTime.Now,
                Tags = new List<string> { "delete" }
            };

            _repository.AddApp(newApp);
            var id = newApp.Id;

            _repository.DeleteApp(id);

            var retrieved = _repository.GetAppById(id);
            retrieved.Should().BeNull();
        }

        [Fact]
        public void Repository_CanDownloadAndUninstallApp()
        {
            // Use an existing user from seed data
            var userId = Guid.Parse("414603a5-19f8-4920-8d61-1fb8384acfb6"); // user
            var repoWithUser = new PostgreSQLRepEF(userId);
            var repoField = typeof(PostgreSQLRepEF).GetField("_db", BindingFlags.NonPublic | BindingFlags.Instance);
            repoField?.SetValue(repoWithUser, _dbContext);

            var apps = repoWithUser.GetAllApps();
            var app = apps.First();

            repoWithUser.DownloadApp(app.Id);
            var downloaded = repoWithUser.GetAppById(app.Id);
            downloaded.Should().NotBeNull();
            downloaded!.IsDownloaded.Should().BeTrue();
            downloaded.DownloadCount.Should().BeGreaterThan(0);

            repoWithUser.UninstallApp(app.Id);
            var uninstalled = repoWithUser.GetAppById(app.Id);
            uninstalled.Should().NotBeNull();
            uninstalled!.IsDownloaded.Should().BeFalse();
        }

        [Fact]
        public void RestoreDefaults_ResetsDatabase()
        {
            // Create a fresh repository for this test to avoid interference from other tests
            var connectionString = Settings.Instance.ConnectionString;
            var options = new DbContextOptionsBuilder<PostgreDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            var dbContext = new PostgreDbContext(options);
            var freshRepo = new PostgreSQLRepEF(Guid.Empty);
            var repoField = typeof(PostgreSQLRepEF).GetField("_db", BindingFlags.NonPublic | BindingFlags.Instance);
            repoField?.SetValue(freshRepo, dbContext);

            // Normalize DB state first — other tests may leave extra rows behind,
            // so capture the baseline AFTER a restore, not before.
            freshRepo.RestoreDefaults();

            // Deterministic baseline = seed data (2 users, 12 apps)
            var baselineUserCount = freshRepo.GetAllUsers().Count;
            var baselineAppCount = freshRepo.GetAllApps().Count;

            // Add test data
            freshRepo.AddApp(new App
            {
                ShortName = "Test",
                FullName = "Test",
                Description = "Test",
                Developer = "Test",
                Category = "Test",
                Rating = 4.0,
                RatingCount = 10,
                Price = 10,
                Version = "1.0",
                SizeMB = 10,
                Country = "US",
                AgeRating = "4+",
                Color = "#FF0000",
                IsFeatured = false,
                IsInStock = true,
                DiscountPercent = null,
                ReleaseDate = DateTime.Now,
                Tags = new List<string> { "test" }
            });
            freshRepo.AddUser(new User { Login = "testuser", PasswordHash = "hash", Role = UserRole.User });

            // Verify data was added
            freshRepo.GetAllApps().Count.Should().Be(baselineAppCount + 1);
            freshRepo.GetAllUsers().Count.Should().Be(baselineUserCount + 1);

            // Restore defaults
            freshRepo.RestoreDefaults();

            // Verify restored to seed state
            freshRepo.GetAllUsers().Count.Should().Be(baselineUserCount);
            freshRepo.GetAllApps().Count.Should().Be(baselineAppCount);
            freshRepo.GetAllUsers().Should().Contain(u => u.Login == "admin");
            freshRepo.GetAllUsers().Should().Contain(u => u.Login == "user");

            dbContext.Dispose();
        }

        // ── QA-001: оценки и регистрация ─────────────────────────────────────

        [Fact]
        public void RateApp_NewRating_PersistsAndUpdatesAggregate()
        {
            var user = _authService.Login("user", "1234")!;
            user.Should().NotBeNull();

            var app = new App
            {
                ShortName = "Rated",
                FullName = "Rated App",
                Description = "Rating target",
                Developer = "Test",
                Category = "Test",
                Rating = 0,
                RatingCount = 0,
                Price = 0,
                Version = "1.0",
                SizeMB = 1,
                Country = "US",
                AgeRating = "4+",
                Color = "#FF0000",
                IsInStock = true,
                ReleaseDate = DateTime.Now,
                Tags = new List<string> { "test" },
            };
            _repository.AddApp(app);

            try
            {
                _repository.RateApp(app.Id, 4);

                _repository.GetUserRating(app.Id).Should().Be(4);
                var stored = _repository.GetAppById(app.Id)!;
                stored.Rating.Should().Be(4.0);
                stored.RatingCount.Should().Be(1);
            }
            finally
            {
                _repository.DeleteApp(app.Id);
            }
        }

        [Fact]
        public void RateApp_SecondRating_ReplacesValueAndKeepsCount()
        {
            var user = _authService.Login("user", "1234")!;
            user.Should().NotBeNull();

            var app = new App
            {
                ShortName = "Rerated",
                FullName = "Rerated App",
                Description = "Rating target",
                Developer = "Test",
                Category = "Test",
                Rating = 0,
                RatingCount = 0,
                Price = 0,
                Version = "1.0",
                SizeMB = 1,
                Country = "US",
                AgeRating = "4+",
                Color = "#FF0000",
                IsInStock = true,
                ReleaseDate = DateTime.Now,
                Tags = new List<string> { "test" },
            };
            _repository.AddApp(app);

            try
            {
                _repository.RateApp(app.Id, 5);
                _repository.RateApp(app.Id, 2);

                _repository.GetUserRating(app.Id).Should().Be(2);
                var stored = _repository.GetAppById(app.Id)!;
                stored.Rating.Should().Be(2.0);
                stored.RatingCount.Should().Be(1);
            }
            finally
            {
                _repository.DeleteApp(app.Id);
            }
        }

        [Fact]
        public void RateApp_Unauthenticated_Throws()
        {
            _repository.SetCurrentUser(null);
            var app = _repository.GetAllApps().First();

            Action act = () => _repository.RateApp(app.Id, 5);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Register_NewUser_CanLoginAndHasIsolatedProfile()
        {
            var login = "reg_" + Guid.NewGuid().ToString("N")[..8];

            var result = _authService.Register(login, "secret123");

            result.Success.Should().BeTrue();
            result.User!.Role.Should().Be(UserRole.User);

            _authService.Logout();
            var loggedIn = _authService.Login(login, "secret123");

            loggedIn.Should().NotBeNull();
            loggedIn!.Login.Should().Be(login);
            loggedIn.FirstName.Should().BeNull();
        }

        [Fact]
        public void RegisteredUser_SeesNoForeignInstalledApps()
        {
            var login = "iso_" + Guid.NewGuid().ToString("N")[..8];
            var result = _authService.Register(login, "secret123");
            result.Success.Should().BeTrue();

            var apps = _repository.GetAllApps();

            apps.Should().NotBeEmpty();
            apps.Should().OnlyContain(a => !a.IsDownloaded);
        }
    }
}