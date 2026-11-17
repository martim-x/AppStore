using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using AppStore.Core.Configuration;

namespace AppStore.Core.Data
{
    public class PostgreDbContext : DbContext
    {
        public DbSet<App> Apps { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UsersApps> UserAppLinks { get; set; } = null!;
        public DbSet<AppRating> AppRatings { get; set; } = null!;
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;

        // Справочники каталога
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<Developer> Developers { get; set; } = null!;
        public DbSet<CatalogCategory> Categories { get; set; } = null!;
        public DbSet<AgeRating> AgeRatings { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<AppTag> AppTags { get; set; } = null!;

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
                e.Property(a => a.DescriptionEn).HasColumnName("description_en");
                e.Property(a => a.Rating).HasColumnName("rating");
                e.Property(a => a.RatingCount).HasColumnName("rating_count");
                e.Property(a => a.Price).HasColumnName("price");
                e.Property(a => a.Version).HasColumnName("version");
                e.Property(a => a.SizeMB).HasColumnName("size_mb");
                e.Property(a => a.Color).HasColumnName("color");
                e.Property(a => a.IsFeatured).HasColumnName("is_featured");
                e.Property(a => a.IsInStock).HasColumnName("is_in_stock");
                e.Property(a => a.DownloadCount).HasColumnName("download_count");
                e.Property(a => a.DiscountPercent).HasColumnName("discount_percent");
                e.Property(a => a.ReleaseDate)
                    .HasColumnName("release_date")
                    .HasColumnType("timestamp without time zone");

                // Справочники: приложение ссылается на категорию, разработчика,
                // страну и возрастную маркировку по внешним ключам.
                e.Property(a => a.DeveloperId).HasColumnName("developer_id");
                e.Property(a => a.CategoryId).HasColumnName("category_id");
                e.Ignore(a => a.CountryId);                e.Ignore(a => a.Developer);
                e.Ignore(a => a.Category);
                e.Ignore(a => a.Country);
                e.Ignore(a => a.AgeRating);
                e.Ignore(a => a.Tags);
                e.Property(a => a.AgeRatingId).HasColumnName("age_rating_id");

                e.HasOne<Developer>()
                    .WithMany()
                    .HasForeignKey(a => a.DeveloperId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne<CatalogCategory>()
                    .WithMany()
                    .HasForeignKey(a => a.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<AgeRating>()
                    .WithMany()
                    .HasForeignKey(a => a.AgeRatingId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(a => a.CategoryId).HasDatabaseName("ix_apps_category");
                e.HasIndex(a => a.DeveloperId).HasDatabaseName("ix_apps_developer");

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
                e.Property(u => u.Role).HasColumnName("role");
                e.Property(u => u.ProfileId).HasColumnName("profile_id");

                // персональные данные и настройки живут в user_profiles
                e.Ignore(u => u.FirstName);
                e.Ignore(u => u.LastName);
                e.Ignore(u => u.Email);
                e.Ignore(u => u.AvatarColor);
                e.Ignore(u => u.Language);
                e.Ignore(u => u.Theme);
                e.Ignore(u => u.Profile);

                // Уникальность логина: без индекса AddUser может вставить
                // дубли, а GetUserByLogin (FirstOrDefault) тогда выдаёт
                // неоднозначный результат. ADO-вариант схемы объявляет
                // login text not null unique – EF-модель обязана совпадать.
                e.HasIndex(u => u.Login).IsUnique().HasDatabaseName("IX_users_login");

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

            modelBuilder.Entity<AppRating>(e =>
            {
                e.ToTable("app_ratings", t =>
                    t.HasCheckConstraint("ck_app_ratings_value", "value between 1 and 5"));

                e.HasKey(x => new { x.UserId, x.AppId });

                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.AppId).HasColumnName("app_id");
                e.Property(x => x.Value).HasColumnName("value");
                e.Property(x => x.RatedAt)
                    .HasColumnName("rated_at")
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

            modelBuilder.Entity<UserProfile>(e =>
            {
                e.ToTable("user_profiles");
                e.HasKey(p => p.Id);
                e.Property(p => p.Id).HasColumnName("id");
                e.Property(p => p.UserId).HasColumnName("user_id");
                e.Property(p => p.FirstName).HasColumnName("first_name").HasMaxLength(80);
                e.Property(p => p.LastName).HasColumnName("last_name").HasMaxLength(80);
                e.Property(p => p.Email).HasColumnName("email").HasMaxLength(120);
                e.Property(p => p.AvatarColor).HasColumnName("avatar_color").HasMaxLength(9);
                e.Property(p => p.Language).HasColumnName("language").HasMaxLength(5);
                e.Property(p => p.Theme).HasColumnName("theme").HasMaxLength(10);
                e.HasIndex(p => p.UserId).IsUnique().HasDatabaseName("ux_user_profiles_user");
                e.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Справочники каталога ────────────────────────────────────────
            modelBuilder.Entity<Country>(e =>
            {
                e.ToTable("countries");
                e.HasKey(c => c.Id);
                e.Property(c => c.Id).HasColumnName("id");
                e.Property(c => c.IsoCode).HasColumnName("iso_code").HasMaxLength(2);
                e.Property(c => c.Name).HasColumnName("name").HasMaxLength(80);
                e.HasIndex(c => c.IsoCode).IsUnique().HasDatabaseName("ux_countries_iso");
                e.Ignore(c => c.Developers);
                e.Ignore(c => c.Apps);
            });

            modelBuilder.Entity<Developer>(e =>
            {
                e.ToTable("developers");
                e.HasKey(d => d.Id);
                e.Property(d => d.Id).HasColumnName("id");
                e.Property(d => d.Name).HasColumnName("name").HasMaxLength(120);
                e.Property(d => d.CountryId).HasColumnName("country_id");
                e.Property(d => d.Website).HasColumnName("website").HasMaxLength(200);
                e.Property(d => d.FoundedYear).HasColumnName("founded_year");
                e.HasIndex(d => d.Name).IsUnique().HasDatabaseName("ux_developers_name");
                e.HasOne<Country>()
                    .WithMany()
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.SetNull);
                e.Ignore(d => d.Country);
                e.Ignore(d => d.Apps);
            });

            modelBuilder.Entity<CatalogCategory>(e =>
            {
                e.ToTable("categories");
                e.HasKey(c => c.Id);
                e.Property(c => c.Id).HasColumnName("id");
                e.Property(c => c.Name).HasColumnName("name").HasMaxLength(60);
                e.HasIndex(c => c.Name).IsUnique().HasDatabaseName("ux_categories_name");
                e.Ignore(c => c.Apps);
            });

            modelBuilder.Entity<AgeRating>(e =>
            {
                e.ToTable("age_ratings");
                e.HasKey(a => a.Id);
                e.Property(a => a.Id).HasColumnName("id");
                e.Property(a => a.Label).HasColumnName("label").HasMaxLength(10);
                e.HasIndex(a => a.Label).IsUnique().HasDatabaseName("ux_age_ratings_label");
                e.Ignore(a => a.Apps);
            });

            modelBuilder.Entity<Tag>(e =>
            {
                e.ToTable("tags");
                e.HasKey(t => t.Id);
                e.Property(t => t.Id).HasColumnName("id");
                e.Property(t => t.Name).HasColumnName("name").HasMaxLength(60);
                e.HasIndex(t => t.Name).IsUnique().HasDatabaseName("ux_tags_name");
                e.Ignore(t => t.Apps);
            });

            modelBuilder.Entity<AppTag>(e =>
            {
                e.ToTable("app_tags");
                e.HasKey(x => new { x.AppId, x.TagId });
                e.Property(x => x.AppId).HasColumnName("app_id");
                e.Property(x => x.TagId).HasColumnName("tag_id");
                e.HasOne<App>()
                    .WithMany()
                    .HasForeignKey(x => x.AppId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey(x => x.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Ignore(x => x.App);
                e.Ignore(x => x.Tag);
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

            // Схема управляется миграциями EF Core. Migrate() применяет миграции
            // и ведёт журнал __EFMigrationsHistory.
            // Для БД, созданной вне миграций (EnsureCreated()/ADO-репозиторий),
            // выполняется baseline: InitialCreate регистрируется как применённая,
            // а последующие миграции (например, уникальный индекс логина) честно
            // применяются. Ошибки миграций не глотаются – сбой виден вызывающему
            // коду и не оставляет БД «молча» без новых миграций.
            BaselineMigrationsIfNeeded();
            _db.Database.Migrate();

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

        /// <summary>
        /// Baseline существующей БД: если схема создана вне миграций
        /// (EnsureCreated()/ADO-репозиторий), таблицы есть, а журнал
        /// __EFMigrationsHistory пуст – регистрируем InitialCreate как
        /// применённую, чтобы Migrate() применил только недостающие миграции.
        /// </summary>
        private void BaselineMigrationsIfNeeded()
        {
            _db.Database.OpenConnection();
            try
            {
                var conn = _db.Database.GetDbConnection();

                bool schemaExists;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'apps')";
                    schemaExists = Convert.ToBoolean(cmd.ExecuteScalar());
                }

                if (!schemaExists)
                    return;

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (" +
                        "\"MigrationId\" character varying(150) NOT NULL, " +
                        "\"ProductVersion\" character varying(32) NOT NULL, " +
                        "CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY (\"MigrationId\"))";
                    cmd.ExecuteNonQuery();
                }

                long historyRows;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM \"__EFMigrationsHistory\"";
                    historyRows = Convert.ToInt64(cmd.ExecuteScalar());
                }

                if (historyRows > 0)
                    return;

                // На baseline-БД (ADO/EnsureCreated-схема) индекса логина может не
                // быть, и таблица может содержать дубликаты логинов. CREATE UNIQUE
                // INDEX миграции упадёт с сырой ошибкой Npgsql – выдаём понятную
                // ошибку с примерами до применения миграции.
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT login FROM \"users\" WHERE login IS NOT NULL " +
                        "GROUP BY login HAVING COUNT(*) > 1 LIMIT 5";
                    using var reader = cmd.ExecuteReader();
                    var duplicates = new System.Collections.Generic.List<string>();
                    while (reader.Read())
                        duplicates.Add(reader.GetString(0));

                    if (duplicates.Count > 0)
                        throw new InvalidOperationException(
                            "Не удаётся применить миграцию AddUniqueIndexOnUserLogin: " +
                            "в таблице users есть дубликаты логинов (" +
                            string.Join(", ", duplicates) + "). " +
                            "Исправьте дубликаты и перезапустите приложение.");
                }

                var initial = _db.Database.GetMigrations().First();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES (@id, @version)";
                    var id = cmd.CreateParameter();
                    id.ParameterName = "id";
                    id.Value = initial;
                    cmd.Parameters.Add(id);
                    var version = cmd.CreateParameter();
                    version.ParameterName = "version";
                    version.Value = "8.0.0";
                    cmd.Parameters.Add(version);
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                _db.Database.CloseConnection();
            }
        }

        public override List<App> GetAllApps()
        {
            var apps = _db.Apps.OrderBy(a => a.FullName).ThenBy(a => a.Id).ToList();
            HydrateNames(apps);

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

            HydrateNames(new List<App> { app });

            app.IsDownloaded =
                _userId != Guid.Empty
                && _db.UserAppLinks.Any(x => x.UserId == _userId && x.AppId == id);

            return app;
        }

        public override void AddApp(App app)
        {
            ApplyDictionaries(app);
            _db.Apps.Add(app);
            _db.SaveChanges();
            ReplaceTags(app);
        }

        public override void UpdateApp(App app)
        {
            var existing = _db.Apps.FirstOrDefault(a => a.Id == app.Id);
            if (existing == null)
                throw new InvalidOperationException($"App with id {app.Id} not found.");

            existing.ShortName = app.ShortName;
            existing.FullName = app.FullName;
            existing.Description = app.Description;
            existing.DescriptionEn = app.DescriptionEn;
            existing.Rating = app.Rating;
            existing.RatingCount = app.RatingCount;
            existing.Price = app.Price;
            existing.Version = app.Version;
            existing.SizeMB = app.SizeMB;
            existing.Color = app.Color;
            existing.IsFeatured = app.IsFeatured;
            existing.IsInStock = app.IsInStock;
            existing.DownloadCount = app.DownloadCount;
            existing.DiscountPercent = app.DiscountPercent;
            existing.ReleaseDate = app.ReleaseDate;

            // Справочные поля приходят из UI строками — приводим к внешним ключам
            existing.DeveloperId = app.DeveloperId;
            existing.CategoryId = app.CategoryId;

            existing.AgeRatingId = app.AgeRatingId;
            ApplyDictionaries(existing, app);
            _db.SaveChanges();

            // Теги перезаписываются целиком (ReplaceTags) — берём список из UI,
            // иначе трекнутая сущность вернёт прежние связи и правка тегов
            // молча потеряется (найдено при smoke-тесте 15.11.2026).
            existing.Tags = app.Tags;
            ReplaceTags(existing);
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

        public override List<User> GetAllUsers()
        {
            var users = _db.Users.OrderBy(u => u.Login).ThenBy(u => u.Id).ToList();
            HydrateProfiles(users);
            return users;
        }

        public override User? GetUserByLogin(string login)
        {
            var user = _db.Users.FirstOrDefault(u => u.Login.ToLower() == login.ToLower());
            if (user != null)
                HydrateProfiles(new List<User> { user });
            return user;
        }

        public override void AddUser(User user)
        {
            var profile = user.Profile ?? new UserProfile();
            profile.UserId = user.Id;
            _db.UserProfiles.Add(profile);
            user.ProfileId = profile.Id;

            _db.Users.Add(user);
            _db.SaveChanges();
            user.Profile = profile;
        }

        public override void UpdateUser(User user)
        {
            var existing = _db.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing == null)
                throw new InvalidOperationException($"User with id {user.Id} not found.");
            existing.Login = user.Login;
            existing.PasswordHash = user.PasswordHash;
            existing.Role = user.Role;

            var profile = _db.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile { UserId = user.Id };
                _db.UserProfiles.Add(profile);
            }

            profile.FirstName = user.FirstName;
            profile.LastName = user.LastName;
            profile.Email = user.Email;
            profile.AvatarColor = user.AvatarColor;
            profile.Language = user.Language;
            profile.Theme = user.Theme;

            existing.ProfileId = profile.Id;
            _db.SaveChanges();
            existing.Profile = profile;
            user.Profile = profile;
        }

        /// <summary>Оценка текущего пользователя для приложения (null — не оценивал).</summary>
        public override int? GetUserRating(Guid appId)
        {
            if (_userId == Guid.Empty)
                return null;

            return _db.AppRatings
                .AsNoTracking()
                .Where(r => r.UserId == _userId && r.AppId == appId)
                .Select(r => (int?)r.Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Поставить или изменить оценку 1–5 (APP-003).
        /// Оценка сохраняется в app_ratings, агрегат apps.rating/rating_count
        /// пересчитывается по накопленной сумме.
        /// </summary>
        public override void RateApp(Guid appId, int value)
        {
            if (_userId == Guid.Empty)
                throw new InvalidOperationException("No user is logged in.");

            if (value < Project.Services.RatingAggregator.MinValue ||
                value > Project.Services.RatingAggregator.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), "Оценка должна быть от 1 до 5.");

            var app = _db.Apps.FirstOrDefault(a => a.Id == appId)
                ?? throw new InvalidOperationException($"App with id {appId} not found.");

            var existing = _db.AppRatings.FirstOrDefault(r => r.UserId == _userId && r.AppId == appId);

            if (existing == null)
            {
                _db.AppRatings.Add(new AppRating
                {
                    UserId = _userId,
                    AppId = appId,
                    Value = value,
                    RatedAt = DateTime.Now,
                });
            }
            else
            {
                existing.Value = value;
                existing.RatedAt = DateTime.Now;
            }

            // Оценка приложения = среднее арифметическое оценок пользователей
            // из app_ratings (решение автора 15.11.2026); seed-значения
            // apps.rating/rating_count больше не участвуют в расчёте.
            // Сначала сохраняем строку оценки, затем берём среднее из БД —
            // иначе запрос не видит несохранённое значение.
            _db.SaveChanges();

            var avg = _db.AppRatings
                .Where(r => r.AppId == appId)
                .Select(r => (int?)r.Value)
                .ToList();

            app.Rating = avg.Count > 0 ? Math.Round(avg.Average(v => v!.Value), 1) : 0;
            app.RatingCount = avg.Count;

            _db.SaveChanges();
        }

        /// <summary>Подтянуть профили (персональные данные и настройки) к пользователям.</summary>
        private void HydrateProfiles(List<User> users)
        {
            if (users.Count == 0)
                return;

            var ids = users.Select(u => u.Id).ToList();
            var profiles = _db.UserProfiles.Where(p => ids.Contains(p.UserId)).ToDictionary(p => p.UserId);

            foreach (var user in users)
            {
                if (profiles.TryGetValue(user.Id, out var profile))
                {
                    user.Profile = profile;
                    user.ProfileId = profile.Id;
                }
                else
                {
                    user.Profile = new UserProfile { UserId = user.Id };
                }
            }
        }

        // ── Работа со справочниками ─────────────────────────────────────────
        //
        // Модель App хранит наименования справочных значений (Developer, Category,
        // Country, AgeRating, Tags) как немаркированные свойства: они удобны для
        // привязок UI. В базе эти значения нормализованы и связаны внешними
        // ключами, поэтому при чтении наименования восстанавливаются из
        // справочников, а при записи приводятся к идентификаторам строк.

        private static readonly Dictionary<string, string> CountryNames = new()
        {
            ["US"] = "США",
            ["RU"] = "Россия",
            ["BY"] = "Беларусь",
            ["DE"] = "Германия",
            ["GB"] = "Великобритания",
            ["SE"] = "Швеция",
            ["JP"] = "Япония",
            ["CA"] = "Канада",
            ["NL"] = "Нидерланды",
            ["AU"] = "Австралия",
            ["FR"] = "Франция",
            ["CN"] = "Китай",
            ["KR"] = "Республика Корея",
            ["UA"] = "Украина",
            ["IL"] = "Израиль",
        };

        /// <summary>Заполнить наименования справочных полей по внешним ключам.</summary>
        private void HydrateNames(List<App> apps)
        {
            if (apps.Count == 0)
                return;

            var categories = _db.Categories.ToDictionary(c => c.Id, c => c.Name);
            var developers = _db.Developers.ToDictionary(d => d.Id, d => d.Name);
            var countries = _db.Countries.ToDictionary(c => c.Id, c => c.IsoCode);
            var ageRatings = _db.AgeRatings.ToDictionary(a => a.Id, a => a.Label);

            var appIds = apps.Select(a => a.Id).ToList();
            var developerCountry = _db
                .Developers.Where(d => d.CountryId != null)
                .ToDictionary(d => d.Id, d => countries.TryGetValue(d.CountryId!.Value, out var iso) ? iso : string.Empty);

            var tagsByApp = _db
                .AppTags.Where(x => appIds.Contains(x.AppId))
                .Join(_db.Tags, x => x.TagId, t => t.Id, (x, t) => new { x.AppId, t.Name })
                .ToList()
                .GroupBy(x => x.AppId)
                .ToDictionary(g => g.Key, g => g.Select(v => v.Name).ToList());

            foreach (var app in apps)
            {
                app.Category =
                    app.CategoryId is Guid cid && categories.TryGetValue(cid, out var categoryName)
                        ? categoryName
                        : string.Empty;
                app.Developer =
                    app.DeveloperId is Guid did && developers.TryGetValue(did, out var developerName)
                        ? developerName
                        : string.Empty;
                app.Country =
                    app.DeveloperId is Guid devId && developerCountry.TryGetValue(devId, out var iso)
                        ? iso
                        : string.Empty;
                app.AgeRating =
                    app.AgeRatingId is Guid aid && ageRatings.TryGetValue(aid, out var label)
                        ? label
                        : string.Empty;
                app.Tags = tagsByApp.TryGetValue(app.Id, out var tags) ? tags : new List<string>();
            }
        }

        /// <summary>
        /// Привести справочные наименования приложения к внешним ключам,
        /// создавая недостающие строки справочников.
        /// </summary>
        private void ApplyDictionaries(App target, App? source = null)
        {
            if (source != null)
            {
                target.Developer = source.Developer;
                target.Category = source.Category;
                target.Country = source.Country;
                target.AgeRating = source.AgeRating;
            }

            if (!string.IsNullOrWhiteSpace(target.Category))
                target.CategoryId = EnsureCategory(target.Category);
            if (!string.IsNullOrWhiteSpace(target.Developer))
                target.DeveloperId = EnsureDeveloper(target.Developer, target.Country);

            if (!string.IsNullOrWhiteSpace(target.AgeRating))
                target.AgeRatingId = EnsureAgeRating(target.AgeRating);
        }

        /// <summary>Переписать связи «приложение — тег» по списку наименований.</summary>
        private void ReplaceTags(App app)
        {
            var links = _db.AppTags.Where(x => x.AppId == app.Id).ToList();
            if (links.Count > 0)
            {
                _db.AppTags.RemoveRange(links);
                _db.SaveChanges();
            }

            foreach (var name in app.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
            {
                _db.AppTags.Add(new AppTag { AppId = app.Id, TagId = EnsureTag(name) });
            }

            _db.SaveChanges();
        }

        private Guid EnsureCountry(string isoCode)
        {
            var iso = isoCode.Trim().ToUpperInvariant();
            var existing = _db.Countries.FirstOrDefault(c => c.IsoCode == iso);
            if (existing != null)
                return existing.Id;

            var country = new Country
            {
                IsoCode = iso,
                Name = CountryNames.TryGetValue(iso, out var name) ? name : iso,
            };
            _db.Countries.Add(country);
            _db.SaveChanges();
            return country.Id;
        }

        private Guid EnsureDeveloper(string name, string? countryIso = null)
        {
            var trimmed = name.Trim();
            var countryId = string.IsNullOrWhiteSpace(countryIso)
                ? (Guid?)null
                : EnsureCountry(countryIso);

            var existing = _db.Developers.FirstOrDefault(d => d.Name == trimmed);
            if (existing != null)
            {
                // страна разработчика хранится здесь, а не в приложении
                if (existing.CountryId == null && countryId != null)
                {
                    existing.CountryId = countryId;
                    _db.SaveChanges();
                }

                return existing.Id;
            }

            var developer = new Developer { Name = trimmed, CountryId = countryId };
            _db.Developers.Add(developer);
            _db.SaveChanges();
            return developer.Id;
        }

        private Guid EnsureCategory(string name)
        {
            var trimmed = name.Trim();
            var existing = _db.Categories.FirstOrDefault(c => c.Name == trimmed);
            if (existing != null)
                return existing.Id;

            var category = new CatalogCategory { Name = trimmed };
            _db.Categories.Add(category);
            _db.SaveChanges();
            return category.Id;
        }

        private Guid EnsureAgeRating(string label)
        {
            var trimmed = label.Trim();
            var existing = _db.AgeRatings.FirstOrDefault(a => a.Label == trimmed);
            if (existing != null)
                return existing.Id;

            var rating = new AgeRating { Label = trimmed };
            _db.AgeRatings.Add(rating);
            _db.SaveChanges();
            return rating.Id;
        }

        private Guid EnsureTag(string name)
        {
            var trimmed = name.Trim();
            var existing = _db.Tags.FirstOrDefault(t => t.Name == trimmed);
            if (existing != null)
                return existing.Id;

            var tag = new Tag { Name = trimmed };
            _db.Tags.Add(tag);
            _db.SaveChanges();
            return tag.Id;
        }

        private void SeedAppsToDb()
        {
            var apps = SeedApps();
            foreach (var app in apps)
            {
                ApplyDictionaries(app);
                _db.Apps.Add(app);
            }

            _db.SaveChanges();

            foreach (var app in apps)
                ReplaceTags(app);
        }

        private void SeedUsersToDb()
        {
            foreach (var user in SeedUsers())
                _db.Users.Add(user);

            _db.SaveChanges();
        }
    }
}
