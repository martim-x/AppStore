using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Project.Data;
using Project.Services;
using Project.Configuration;
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
            // Override Settings before creating repository
            var envVars = new Dictionary<string, string>
            {
                ["DB_HOST"] = "localhost",
                ["DB_PORT_FROM"] = "5433",
                ["DB_NAME"] = "appstore",
                ["DB_ADMIN"] = "postgres",
                ["DB_ADMIN_PASS"] = "changeme"
            };

            foreach (var kvp in envVars)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }

            // Force Settings to reload
            var loadMethod = typeof(Settings).GetMethod("LoadFromEnvironment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadMethod?.Invoke(Settings.Instance, null);

            // Use the actual CompositionRoot to build DI container
            _services = CompositionRoot.Build();

            var connectionString = Settings.Instance.ConnectionString;

            var options = new DbContextOptionsBuilder<PostgreDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            _dbContext = new PostgreDbContext(options);
            _repository = _services.GetRequiredService<PostgreSQLRepEF>();

            // Replace the internal DbContext
            var repoField = typeof(PostgreSQLRepEF).GetField("_db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            repoField?.SetValue(_repository, _dbContext);

            _authService = _services.GetRequiredService<IAuthService>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _dbContext.DisposeAsync();
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

            var retrieved = _repository.GetAppById(newApp.Id);
            retrieved.Should().NotBeNull();
            retrieved!.ShortName.Should().Be("TestApp");
            retrieved.FullName.Should().Be("Test Application");
        }

        [Fact]
        public void Repository_CanUpdateApp()
        {
            var apps = _repository.GetAllApps();
            var app = apps.First();
            var originalName = app.FullName;
            app.FullName = "Updated Name";

            _repository.UpdateApp(app);

            var retrieved = _repository.GetAppById(app.Id);
            retrieved.Should().NotBeNull();
            retrieved!.FullName.Should().Be("Updated Name");
            retrieved.FullName.Should().NotBe(originalName);
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
            var repoField = typeof(PostgreSQLRepEF).GetField("_db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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
                    var repoField = typeof(PostgreSQLRepEF).GetField("_db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    repoField?.SetValue(freshRepo, dbContext);

                    // Capture current state as baseline (dynamic, since other tests may have modified DB)
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

                    // Verify restored to original baseline state
                    freshRepo.GetAllUsers().Count.Should().Be(baselineUserCount);
                    freshRepo.GetAllApps().Count.Should().Be(baselineAppCount);
                    freshRepo.GetAllUsers().Should().Contain(u => u.Login == "admin");
                    freshRepo.GetAllUsers().Should().Contain(u => u.Login == "user");

                    dbContext.Dispose();
                }
    }
}
