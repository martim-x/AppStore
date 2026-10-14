using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project.Configuration;

namespace Project.Data
{
    public class PostgreDbContext : DbContext
    {
        public DbSet<App> Apps { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UsersApps> UserAppLinks { get; set; } = null!;

        public PostgreDbContext(DbContextOptions<PostgreDbContext> options)
            : base(options) { }

        public PostgreDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder
                    .UseNpgsql(Settings.Instance.ConnectionString, npgsql =>
                    {
                        npgsql.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null);
                    });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<App>(e =>
            {
                e.ToTable("apps");

                e.HasKey(a => a.Id);

                e.Property(a => a.Id).HasColumnName("id");
                e.Property(a => a.ShortName).HasColumnName("short_name");
                e.Property(a => a.FullName).HasColumnName("full_name");
                e.Property(a => a.Description).HasColumnName("description");
                e.Property(a => a.Developer).HasColumnName("developer");
                e.Property(a => a.Category).HasColumnName("category");
                e.Property(a => a.Rating).HasColumnName("rating");
                e.Property(a => a.RatingCount).HasColumnName("rating_count");
                e.Property(a => a.Price).HasColumnName("price");
                e.Property(a => a.Version).HasColumnName("version");
                e.Property(a => a.SizeMB).HasColumnName("size_mb");
                e.Property(a => a.Country).HasColumnName("country");
                e.Property(a => a.AgeRating).HasColumnName("age_rating");
                e.Property(a => a.Color).HasColumnName("color");
                e.Property(a => a.IsFeatured).HasColumnName("is_featured");
                e.Property(a => a.IsInStock).HasColumnName("is_in_stock");
                e.Property(a => a.DownloadCount).HasColumnName("download_count");
                e.Property(a => a.DiscountPercent).HasColumnName("discount_percent");
                e.Property(a => a.ReleaseDate)
                    .HasColumnName("release_date")
                    .HasColumnType("timestamp without time zone");

                e.Property(a => a.Tags)
                    .HasColumnName("tags")
                    .HasConversion(
                        v => string.Join(",", v),
                        v =>
                            string.IsNullOrWhiteSpace(v)
                                ? new List<string>()
                                : v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                    )
                    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

                e.Ignore(a => a.FinalPrice);
                e.Ignore(a => a.ButtonLabel);
                e.Ignore(a => a.IsDownloaded);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");

                e.HasKey(u => u.Id);

                e.Property(u => u.Id).HasColumnName("id");
                e.Property(u => u.Login).HasColumnName("login");
                e.Property(u => u.PasswordHash).HasColumnName("password_hash");
                e.Property(u => u.FirstName).HasColumnName("first_name");
                e.Property(u => u.LastName).HasColumnName("last_name");
                e.Property(u => u.Email).HasColumnName("email");
                e.Property(u => u.Role).HasColumnName("role");
                e.Property(u => u.AvatarColor).HasColumnName("avatar_color");

                e.Ignore(u => u.AvatarLetter);
            });

            modelBuilder.Entity<UsersApps>(e =>
            {
                e.ToTable("users_apps");

                e.HasKey(x => new { x.UserId, x.AppId });

                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.AppId).HasColumnName("app_id");
                e.Property(x => x.InstalledAt)
                    .HasColumnName("installed_at")
                    .HasColumnType("timestamp without time zone");

                e.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<App>()
                    .WithMany()
                    .HasForeignKey(x => x.AppId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    public class PostgreSQLRepEF : BaseRepository
    {
        private readonly PostgreDbContext _db;
        private Guid _userId;

        public PostgreSQLRepEF(Guid currentUserId = default)
        {
            _userId = currentUserId;

            var options = new DbContextOptionsBuilder<PostgreDbContext>()
                .UseNpgsql(Settings.Instance.ConnectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                })
                .Options;

            _db = new PostgreDbContext(options);

            // Database is managed by migrations, not EnsureDatabaseCreated
            // Ensure database is created (for first run if migrations not applied)
            _db.Database.EnsureCreated();

            // Seed data only if tables are empty
            if (!_db.Apps.Any())
                SeedAppsToDb();

            if (!_db.Users.Any())
                SeedUsersToDb();
        }

        public void SetCurrentUser(Guid? userId)
        {
            _userId = userId ?? Guid.Empty;
        }

        public override List<App> GetAllApps()
        {
            var apps = _db.Apps.OrderBy(a => a.FullName).ThenBy(a => a.Id).ToList();

            var installedIds =
                _userId == Guid.Empty
                    ? new HashSet<Guid>()
                    : _db
                        .UserAppLinks.Where(x => x.UserId == _userId)
                        .Select(x => x.AppId)
                        .ToHashSet();

            foreach (var app in apps)
                app.IsDownloaded = installedIds.Contains(app.Id);

            return apps;
        }

        public override App? GetAppById(Guid id)
        {
            var app = _db.Apps.FirstOrDefault(a => a.Id == id);
            if (app == null)
                return null;

            app.IsDownloaded =
                _userId != Guid.Empty
                && _db.UserAppLinks.Any(x => x.UserId == _userId && x.AppId == id);

            return app;
        }

        public override void AddApp(App app)
        {
            _db.Apps.Add(app);
            _db.SaveChanges();
        }

        public override void UpdateApp(App app)
        {
            var existing = _db.Apps.FirstOrDefault(a => a.Id == app.Id);
            if (existing == null)
                throw new InvalidOperationException($"App with id {app.Id} not found.");

            existing.ShortName = app.ShortName;
            existing.FullName = app.FullName;
            existing.Description = app.Description;
            existing.Developer = app.Developer;
            existing.Category = app.Category;
            existing.Rating = app.Rating;
            existing.RatingCount = app.RatingCount;
            existing.Price = app.Price;
            existing.Version = app.Version;
            existing.SizeMB = app.SizeMB;
            existing.Country = app.Country;
            existing.AgeRating = app.AgeRating;
            existing.Color = app.Color;
            existing.IsFeatured = app.IsFeatured;
            existing.IsInStock = app.IsInStock;
            existing.DownloadCount = app.DownloadCount;
            existing.DiscountPercent = app.DiscountPercent;
            existing.ReleaseDate = app.ReleaseDate;
            existing.Tags = app.Tags;

            _db.SaveChanges();
        }

        public override void DeleteApp(Guid id)
        {
            var item = _db.Apps.FirstOrDefault(a => a.Id == id);
            if (item == null)
                return;

            var links = _db.UserAppLinks.Where(x => x.AppId == id).ToList();
            if (links.Count > 0)
                _db.UserAppLinks.RemoveRange(links);

            _db.Apps.Remove(item);
            _db.SaveChanges();
        }

        public override void DownloadApp(Guid appId)
        {
            if (_userId == Guid.Empty)
                return;

            var app = _db.Apps.FirstOrDefault(a => a.Id == appId);
            if (app == null)
                return;

            var record = _db.UserAppLinks.Find(_userId, appId);

            if (record == null)
            {
                _db.UserAppLinks.Add(
                    new UsersApps
                    {
                        UserId = _userId,
                        AppId = appId,
                        InstalledAt = DateTime.Now,
                    }
                );

                app.DownloadCount++;
            }
            else
            {
                record.InstalledAt = DateTime.Now;
            }

            _db.SaveChanges();
        }

        public override void UninstallApp(Guid appId)
        {
            if (_userId == Guid.Empty)
                return;

            var app = _db.Apps.FirstOrDefault(a => a.Id == appId);
            if (app == null)
                return;

            var record = _db.UserAppLinks.Find(_userId, appId);
            if (record == null)
                return;

            _db.UserAppLinks.Remove(record);

            if (app.DownloadCount > 0)
                app.DownloadCount--;

            _db.SaveChanges();
        }

        public override void RestoreDefaults()
        {
            _db.UserAppLinks.RemoveRange(_db.UserAppLinks);
            _db.Apps.RemoveRange(_db.Apps);
            _db.Users.RemoveRange(_db.Users);
            _db.SaveChanges();

            SeedUsersToDb();
            SeedAppsToDb();
        }

        public override List<User> GetAllUsers() =>
            _db.Users.OrderBy(u => u.Login).ThenBy(u => u.Id).ToList();

        public override User? GetUserByLogin(string login) =>
            _db.Users.FirstOrDefault(u => u.Login.ToLower() == login.ToLower());

        public override void AddUser(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
        }

        public override void UpdateUser(User user)
        {
            var existing = _db.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing == null)
                throw new InvalidOperationException($"User with id {user.Id} not found.");

            existing.Login = user.Login;
            existing.PasswordHash = user.PasswordHash;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.AvatarColor = user.AvatarColor;

            _db.SaveChanges();
        }

        private void SeedAppsToDb()
        {
            foreach (var app in SeedApps())
                _db.Apps.Add(app);

            _db.SaveChanges();
        }

        private void SeedUsersToDb()
        {
            foreach (var user in SeedUsers())
                _db.Users.Add(user);

            _db.SaveChanges();
        }
    }
}
