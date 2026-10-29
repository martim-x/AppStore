using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace AppStore.Core.Data
{
    public class PostgreSQLRepADO : BaseRepository
    {
        private readonly string _connectionString;
        private Guid _userId;

        public PostgreSQLRepADO(Guid currentUserId = default)
        {
            _userId = currentUserId;
            _connectionString = LoadConnectionString();

            EnsureDatabaseCreated();
            EnsureSeeded();
        }

        public void SetCurrentUser(Guid? userId)
        {
            _userId = userId ?? Guid.Empty;
        }

        public static string LoadConnectionString()
        {
            var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
            if (!File.Exists(envPath))
                envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

            string host = "0.0.0.0",
                port = "5432",
                database = "postgres",
                username = "postgres",
                password = "111";

            if (File.Exists(envPath))
            {
                foreach (var raw in File.ReadAllLines(envPath))
                {
                    var line = raw.Trim();
                    if (line.StartsWith('#') || !line.Contains('='))
                        continue;

                    var eqIdx = line.IndexOf('=');
                    var key = line[..eqIdx].Trim();
                    var val = line[(eqIdx + 1)..].Trim().Trim('"');

                    switch (key)
                    {
                        case "DB_HOST":
                            host = val;
                            break;
                        case "DB_PORT_FROM":
                            port = val;
                            break;
                        case "DB_NAME":
                            database = val;
                            break;
                        case "DB_ADMIN":
                            username = val;
                            break;
                        case "DB_ADMIN_PASS":
                            password = val;
                            break;
                    }
                }
            }

            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        // =========================
        // Apps (синхронные)
        // =========================

        public override List<App> GetAllApps()
        {
            var apps = new List<App>();
            var installedIds = GetInstalledAppIds();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            const string sql = """
                select a.id, a.short_name, a.full_name, a.description,
                       coalesce(d.name, '') as developer,
                       coalesce(c.name, '') as category,
                       a.rating, a.rating_count, a.price, a.version, a.size_mb,
                       coalesce(n.iso_code, '') as country,
                       coalesce(r.label, '') as age_rating,
                       a.color, a.is_featured, a.is_in_stock, a.download_count,
                       a.discount_percent, a.release_date,
                       coalesce((
                           select string_agg(t.name, ',' order by t.name)
                           from app_tags x join tags t on t.id = x.tag_id
                           where x.app_id = a.id), '') as tags,
                       a.logo
                from apps a
                left join developers d on d.id = a.developer_id
                left join categories c on c.id = a.category_id
                left join countries n on n.id = d.country_id
                left join age_ratings r on r.id = a.age_rating_id
                order by full_name, id
                """;

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var app = MapApp(reader);
                app.IsDownloaded = installedIds.Contains(app.Id);
                apps.Add(app);
            }

            return apps;
        }

        public override App? GetAppById(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            const string sql = """
                select a.id, a.short_name, a.full_name, a.description,
                       coalesce(d.name, '') as developer,
                       coalesce(c.name, '') as category,
                       a.rating, a.rating_count, a.price, a.version, a.size_mb,
                       coalesce(n.iso_code, '') as country,
                       coalesce(r.label, '') as age_rating,
                       a.color, a.is_featured, a.is_in_stock, a.download_count,
                       a.discount_percent, a.release_date,
                       coalesce((
                           select string_agg(t.name, ',' order by t.name)
                           from app_tags x join tags t on t.id = x.tag_id
                           where x.app_id = a.id), '') as tags,
                       a.logo
                from apps a
                left join developers d on d.id = a.developer_id
                left join categories c on c.id = a.category_id
                left join countries n on n.id = d.country_id
                left join age_ratings r on r.id = a.age_rating_id
                where id = @id
                limit 1
                """;

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            var app = MapApp(reader);
            app.IsDownloaded = IsAppDownloaded(id);
            return app;
        }

        public override void AddApp(App app)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                ResolveDictionaries(connection, tx, app);

                const string sql = """
                    insert into apps
                    (
                        id, short_name, full_name, description, developer_id, category_id,
                        country_id, age_rating_id, rating, rating_count, price, version,
                        size_mb, color, is_featured, is_in_stock, download_count,
                        discount_percent, release_date, logo
                    )
                    values
                    (
                        @id, @short_name, @full_name, @description, @developer_id, @category_id,
                        @country_id, @age_rating_id, @rating, @rating_count, @price, @version,
                        @size_mb, @color, @is_featured, @is_in_stock, @download_count,
                        @discount_percent, @release_date, @logo
                    )
                    """;

                using var command = new NpgsqlCommand(sql, connection, tx);
                FillAppParameters(command, app);

                var logoBytes = CreateSolidLogoBytes(40, 40, 0x33, 0x99, 0xFF);
                command.Parameters.AddWithValue("logo", logoBytes ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
                ReplaceTags(connection, tx, app);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public override void UpdateApp(App app)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                ResolveDictionaries(connection, tx, app);

                const string sql = """
                    update apps
                    set short_name = @short_name,
                        full_name = @full_name,
                        description = @description,
                        developer_id = @developer_id,
                        category_id = @category_id,
                        country_id = @country_id,
                        age_rating_id = @age_rating_id,
                        rating = @rating,
                        rating_count = @rating_count,
                        price = @price,
                        version = @version,
                        size_mb = @size_mb,
                        color = @color,
                        is_featured = @is_featured,
                        is_in_stock = @is_in_stock,
                        download_count = @download_count,
                        discount_percent = @discount_percent,
                        release_date = @release_date
                    where id = @id
                    """;

                using var command = new NpgsqlCommand(sql, connection, tx);
                FillAppParameters(command, app);

                var rows = command.ExecuteNonQuery();
                if (rows == 0)
                    throw new InvalidOperationException($"App with id {app.Id} not found.");

                ReplaceTags(connection, tx, app);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public override void DeleteApp(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                using (
                    var deleteLinks = new NpgsqlCommand(
                        "delete from users_apps where app_id = @app_id",
                        connection,
                        tx
                    )
                )
                {
                    deleteLinks.Parameters.AddWithValue("app_id", id);
                    deleteLinks.ExecuteNonQuery();
                }

                using (
                    var deleteApp = new NpgsqlCommand(
                        "delete from apps where id = @id",
                        connection,
                        tx
                    )
                )
                {
                    deleteApp.Parameters.AddWithValue("id", id);
                    deleteApp.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public override void DownloadApp(Guid id)
        {
            if (_userId == Guid.Empty)
                return;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                bool appExists;
                using (
                    var appExistsCmd = new NpgsqlCommand(
                        "select exists(select 1 from apps where id = @id)",
                        connection,
                        tx
                    )
                )
                {
                    appExistsCmd.Parameters.AddWithValue("id", id);
                    appExists = (bool)appExistsCmd.ExecuteScalar()!;
                }

                if (!appExists)
                {
                    tx.Rollback();
                    return;
                }

                bool recordExists;
                using (
                    var recordExistsCmd = new NpgsqlCommand(
                        "select exists(select 1 from users_apps where user_id = @user_id and app_id = @app_id)",
                        connection,
                        tx
                    )
                )
                {
                    recordExistsCmd.Parameters.AddWithValue("user_id", _userId);
                    recordExistsCmd.Parameters.AddWithValue("app_id", id);
                    recordExists = (bool)recordExistsCmd.ExecuteScalar()!;
                }

                if (!recordExists)
                {
                    using var insertLink = new NpgsqlCommand(
                        """
                        insert into users_apps (user_id, app_id, installed_at)
                        values (@user_id, @app_id, @installed_at)
                        """,
                        connection,
                        tx
                    );

                    insertLink.Parameters.AddWithValue("user_id", _userId);
                    insertLink.Parameters.AddWithValue("app_id", id);
                    insertLink.Parameters.AddWithValue("installed_at", DateTime.UtcNow);
                    insertLink.ExecuteNonQuery();

                    using var incDownloads = new NpgsqlCommand(
                        "update apps set download_count = download_count + 1 where id = @id",
                        connection,
                        tx
                    );

                    incDownloads.Parameters.AddWithValue("id", id);
                    incDownloads.ExecuteNonQuery();
                }
                else
                {
                    using var updateLink = new NpgsqlCommand(
                        """
                        update users_apps
                        set installed_at = @installed_at
                        where user_id = @user_id and app_id = @app_id
                        """,
                        connection,
                        tx
                    );

                    updateLink.Parameters.AddWithValue("user_id", _userId);
                    updateLink.Parameters.AddWithValue("app_id", id);
                    updateLink.Parameters.AddWithValue("installed_at", DateTime.UtcNow);
                    updateLink.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public override void UninstallApp(Guid id)
        {
            if (_userId == Guid.Empty)
                return;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                int deletedRows;
                using (
                    var deleteLink = new NpgsqlCommand(
                        """
                        delete from users_apps
                        where user_id = @user_id and app_id = @app_id
                        """,
                        connection,
                        tx
                    )
                )
                {
                    deleteLink.Parameters.AddWithValue("user_id", _userId);
                    deleteLink.Parameters.AddWithValue("app_id", id);
                    deletedRows = deleteLink.ExecuteNonQuery();
                }

                if (deletedRows > 0)
                {
                    using var decDownloads = new NpgsqlCommand(
                        """
                        update apps
                        set download_count = case
                            when download_count > 0 then download_count - 1
                            else 0
                        end
                        where id = @id
                        """,
                        connection,
                        tx
                    );

                    decDownloads.Parameters.AddWithValue("id", id);
                    decDownloads.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public override void RestoreDefaults()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                using (var clearLinks = new NpgsqlCommand("delete from users_apps", connection, tx))
                    clearLinks.ExecuteNonQuery();

                using (var clearApps = new NpgsqlCommand("delete from apps", connection, tx))
                    clearApps.ExecuteNonQuery();

                using (var clearUsers = new NpgsqlCommand("delete from users", connection, tx))
                    clearUsers.ExecuteNonQuery();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            SeedUsersToDb();
            SeedAppsToDb();
        }

        // =========================
        // Users (синхронные)
        // =========================

        public override List<User> GetAllUsers()
        {
            var users = new List<User>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            const string sql = """
                select u.id, u.login, u.password_hash, u.role,
                       coalesce(p.first_name, '') as first_name,
                       coalesce(p.last_name, '') as last_name,
                       coalesce(p.email, '') as email,
                       coalesce(p.avatar_color, '#007AFF') as avatar_color,
                       coalesce(p.language, 'ru') as language,
                       coalesce(p.theme, 'light') as theme,
                       p.id as profile_id
                from users u
                left join user_profiles p on p.user_id = u.id
                order by login, id
                """;

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
                users.Add(MapUser(reader));

            return users;
        }

        public override User? GetUserByLogin(string login)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            const string sql = """
                select u.id, u.login, u.password_hash, u.role,
                       coalesce(p.first_name, '') as first_name,
                       coalesce(p.last_name, '') as last_name,
                       coalesce(p.email, '') as email,
                       coalesce(p.avatar_color, '#007AFF') as avatar_color,
                       coalesce(p.language, 'ru') as language,
                       coalesce(p.theme, 'light') as theme,
                       p.id as profile_id
                from users u
                left join user_profiles p on p.user_id = u.id
                where lower(login) = lower(@login)
                limit 1
                """;

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("login", login);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return MapUser(reader);
        }

        public override void AddUser(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                const string sql = """
                    insert into users (id, login, password_hash, role)
                    values (@id, @login, @password_hash, @role)
                    """;

                using var command = new NpgsqlCommand(sql, connection, tx);
                FillUserParameters(command, user);
                command.ExecuteNonQuery();

                UpsertProfile(connection, tx, user);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public override void UpdateUser(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                const string sql = """
                    update users
                    set login = @login,
                        password_hash = @password_hash,
                        role = @role
                    where id = @id
                    """;

                using var command = new NpgsqlCommand(sql, connection, tx);
                FillUserParameters(command, user);
                command.ExecuteNonQuery();

                UpsertProfile(connection, tx, user);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // =========================
        // Инфраструктура
        // =========================

        private void EnsureDatabaseCreated()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            const string sql = """
                create table if not exists countries
                (
                    id uuid primary key,
                    iso_code text not null unique,
                    name text not null
                );

                create table if not exists categories
                (
                    id uuid primary key,
                    name text not null unique
                );

                create table if not exists age_ratings
                (
                    id uuid primary key,
                    label text not null unique
                );

                create table if not exists developers
                (
                    id uuid primary key,
                    name text not null unique,
                    country_id uuid null references countries(id) on delete set null,
                    website text null,
                    founded_year integer null
                );

                create table if not exists tags
                (
                    id uuid primary key,
                    name text not null unique
                );

                create table if not exists apps
                (
                    id uuid primary key,
                    short_name text not null,
                    full_name text not null,
                    description text not null,
                    developer_id uuid null references developers(id),
                    category_id uuid null references categories(id),
                    country_id uuid null references countries(id),
                    age_rating_id uuid null references age_ratings(id),
                    rating double precision not null,
                    rating_count integer not null,
                    price double precision not null,
                    version text not null,
                    size_mb double precision not null,
                    color text not null,
                    is_featured boolean not null,
                    is_in_stock boolean not null,
                    download_count integer not null,
                    discount_percent double precision null,
                    release_date timestamp without time zone not null,
                    logo bytea null
                );

                create table if not exists app_tags
                (
                    app_id uuid not null references apps(id) on delete cascade,
                    tag_id uuid not null references tags(id) on delete cascade,
                    primary key (app_id, tag_id)
                );

                create table if not exists users
                (
                    id uuid primary key,
                    login text not null unique,
                    password_hash text not null,
                    role integer not null
                );

                create table if not exists user_profiles
                (
                    id uuid primary key,
                    user_id uuid not null unique references users(id) on delete cascade,
                    first_name text null,
                    last_name text null,
                    email text null,
                    avatar_color text not null default '#007AFF',
                    language text not null default 'ru',
                    theme text not null default 'light'
                );

                alter table users
                    add column if not exists profile_id uuid null references user_profiles(id);

                create table if not exists users_apps
                (
                    user_id uuid not null,
                    app_id uuid not null,
                    installed_at timestamp without time zone not null,
                    primary key (user_id, app_id),
                    constraint fk_users_apps_user
                        foreign key (user_id) references users(id) on delete cascade,
                    constraint fk_users_apps_app
                        foreign key (app_id) references apps(id) on delete cascade
                );
                """;

            using var command = new NpgsqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private void EnsureSeeded()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            bool hasApps;
            bool hasUsers;

            using (var appsCmd = new NpgsqlCommand("select exists(select 1 from apps)", connection))
                hasApps = (bool)appsCmd.ExecuteScalar()!;

            using (
                var usersCmd = new NpgsqlCommand("select exists(select 1 from users)", connection)
            )
                hasUsers = (bool)usersCmd.ExecuteScalar()!;

            if (!hasApps)
                SeedAppsToDb();

            if (!hasUsers)
                SeedUsersToDb();
        }

        private void SeedAppsToDb()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            foreach (var app in SeedApps())
            {
                ResolveDictionaries(connection, null, app);

                using var command = new NpgsqlCommand(
                    """
                    insert into apps
                    (
                        id, short_name, full_name, description, developer_id, category_id,
                        country_id, age_rating_id, rating, rating_count, price, version,
                        size_mb, color, is_featured, is_in_stock, download_count,
                        discount_percent, release_date, logo
                    )
                    values
                    (
                        @id, @short_name, @full_name, @description, @developer_id, @category_id,
                        @country_id, @age_rating_id, @rating, @rating_count, @price, @version,
                        @size_mb, @color, @is_featured, @is_in_stock, @download_count,
                        @discount_percent, @release_date, @logo
                    )
                    """,
                    connection
                );

                FillAppParameters(command, app);
                var logoBytes = CreateSolidLogoBytes(40, 40, 0x33, 0x99, 0xFF);
                command.Parameters.AddWithValue("logo", logoBytes ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
                ReplaceTags(connection, null, app);
            }
        }

        private void SeedUsersToDb()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            foreach (var user in SeedUsers())
            {
                using var command = new NpgsqlCommand(
                    """
                    insert into users (id, login, password_hash, role)
                    values (@id, @login, @password_hash, @role)
                    """,
                    connection
                );

                FillUserParameters(command, user);
                command.ExecuteNonQuery();
                UpsertProfile(connection, null, user);
            }
        }

        private HashSet<Guid> GetInstalledAppIds()
        {
            var result = new HashSet<Guid>();

            if (_userId == Guid.Empty)
                return result;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            const string sql = """
                select app_id
                from users_apps
                where user_id = @user_id
                """;

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("user_id", _userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetGuid(0));

            return result;
        }

        private bool IsAppDownloaded(Guid appId)
        {
            if (_userId == Guid.Empty)
                return false;

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            const string sql = """
                select exists(
                    select 1
                    from users_apps
                    where user_id = @user_id and app_id = @app_id
                )
                """;

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("user_id", _userId);
            command.Parameters.AddWithValue("app_id", appId);

            return (bool)command.ExecuteScalar()!;
        }

        private static App MapApp(NpgsqlDataReader reader)
        {
            return new App
            {
                Id = reader.GetGuid(0),
                ShortName = reader.GetString(1),
                FullName = reader.GetString(2),
                Description = reader.GetString(3),
                Developer = reader.GetString(4),
                Category = reader.GetString(5),
                Rating = reader.GetDouble(6),
                RatingCount = reader.GetInt32(7),
                Price = reader.GetDouble(8),
                Version = reader.GetString(9),
                SizeMB = reader.GetDouble(10),
                Country = reader.GetString(11),
                AgeRating = reader.GetString(12),
                Color = reader.GetString(13),
                IsFeatured = reader.GetBoolean(14),
                IsInStock = reader.GetBoolean(15),
                DownloadCount = reader.GetInt32(16),
                DiscountPercent = reader.IsDBNull(17) ? null : reader.GetDouble(17),
                ReleaseDate = reader.GetDateTime(18),
                Tags = ParseTags(reader.GetString(19)),
            };
        }


        /// <summary>
        /// Найти или создать строку справочника по бизнес-ключу и вернуть её id.
        /// </summary>
        private static Guid? ResolveRef(
            NpgsqlConnection connection,
            NpgsqlTransaction? tx,
            string table,
            string keyColumn,
            string? value,
            string? nameValue = null
        )
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var sql = nameValue == null
                ? $"insert into {table} (id, {keyColumn}) values (@id, @key) "
                  + $"on conflict ({keyColumn}) do update set {keyColumn} = excluded.{keyColumn} returning id"
                : $"insert into {table} (id, {keyColumn}, name) values (@id, @key, @name) "
                  + $"on conflict ({keyColumn}) do update set {keyColumn} = excluded.{keyColumn} returning id";

            using var command = new NpgsqlCommand(sql, connection, tx);
            var id = Guid.NewGuid();
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("key", value.Trim());
            if (nameValue != null)
                command.Parameters.AddWithValue("name", nameValue.Trim());

            return (Guid)command.ExecuteScalar()!;
        }

        /// <summary>Разрешить справочные наименования приложения в идентификаторы.</summary>
        private static void ResolveDictionaries(
            NpgsqlConnection connection,
            NpgsqlTransaction? tx,
            App app
        )
        {
            app.DeveloperId = ResolveRef(connection, tx, "developers", "name", app.Developer);
            app.CategoryId = ResolveRef(connection, tx, "categories", "name", app.Category);

            // страна приложения хранится у разработчика
            if (!string.IsNullOrWhiteSpace(app.Country) && app.DeveloperId is Guid developerId)
            {
                var iso = app.Country.Trim().ToUpperInvariant();
                var countryId = ResolveRef(connection, tx, "countries", "iso_code", iso, iso);

                using var setCountry = new NpgsqlCommand(
                    "update developers set country_id = @cid where id = @did and country_id is null",
                    connection, tx);
                setCountry.Parameters.AddWithValue("cid", countryId);
                setCountry.Parameters.AddWithValue("did", developerId);
                setCountry.ExecuteNonQuery();
            }
            app.AgeRatingId = ResolveRef(connection, tx, "age_ratings", "label", app.AgeRating);
        }

        /// <summary>Переписать связи «приложение — тег» по списку наименований.</summary>
        private static void ReplaceTags(
            NpgsqlConnection connection,
            NpgsqlTransaction? tx,
            App app
        )
        {
            using (
                var delete = new NpgsqlCommand(
                    "delete from app_tags where app_id = @id", connection, tx)
            )
            {
                delete.Parameters.AddWithValue("id", app.Id);
                delete.ExecuteNonQuery();
            }

            foreach (var name in app.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
            {
                var tagId = ResolveRef(connection, tx, "tags", "name", name);
                if (tagId == null)
                    continue;

                using var insert = new NpgsqlCommand(
                    "insert into app_tags (app_id, tag_id) values (@app_id, @tag_id) on conflict do nothing",
                    connection,
                    tx
                );
                insert.Parameters.AddWithValue("app_id", app.Id);
                insert.Parameters.AddWithValue("tag_id", tagId.Value);
                insert.ExecuteNonQuery();
            }
        }


        /// <summary>Создать или обновить профиль пользователя.</summary>
        private static void UpsertProfile(
            NpgsqlConnection connection, NpgsqlTransaction? tx, User user)
        {
            const string sql = """
                insert into user_profiles
                    (id, user_id, first_name, last_name, email, avatar_color, language, theme)
                values (@id, @user_id, @first_name, @last_name, @email, @avatar_color, @language, @theme)
                on conflict (user_id) do update
                set first_name = excluded.first_name,
                    last_name = excluded.last_name,
                    email = excluded.email,
                    avatar_color = excluded.avatar_color,
                    language = excluded.language,
                    theme = excluded.theme
                returning id
                """;

            using var command = new NpgsqlCommand(sql, connection, tx);
            command.Parameters.AddWithValue("id", Guid.NewGuid());
            command.Parameters.AddWithValue("user_id", user.Id);
            command.Parameters.AddWithValue("first_name", (object?)user.FirstName ?? DBNull.Value);
            command.Parameters.AddWithValue("last_name", (object?)user.LastName ?? DBNull.Value);
            command.Parameters.AddWithValue("email", (object?)user.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("avatar_color", user.AvatarColor);
            command.Parameters.AddWithValue("language", user.Language);
            command.Parameters.AddWithValue("theme", user.Theme);

            var savedId = (Guid)command.ExecuteScalar()!;

            using var link = new NpgsqlCommand(
                "update users set profile_id = @pid where id = @uid", connection, tx);
            link.Parameters.AddWithValue("pid", savedId);
            link.Parameters.AddWithValue("uid", user.Id);
            link.ExecuteNonQuery();

            user.ProfileId = savedId;
        }

        private static User MapUser(NpgsqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Role = (UserRole)reader.GetInt32(3),
                ProfileId = reader.IsDBNull(10) ? null : reader.GetGuid(10),
                Profile = new UserProfile
                {
                    UserId = reader.GetGuid(0),
                    FirstName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LastName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Email = reader.IsDBNull(6) ? null : reader.GetString(6),
                    AvatarColor = reader.IsDBNull(7) ? "#007AFF" : reader.GetString(7),
                    Language = reader.IsDBNull(8) ? "ru" : reader.GetString(8),
                    Theme = reader.IsDBNull(9) ? "light" : reader.GetString(9),
                },
            };
        }

        private static void FillAppParameters(NpgsqlCommand command, App app)
        {
            command.Parameters.AddWithValue("id", app.Id);
            command.Parameters.AddWithValue("short_name", app.ShortName);
            command.Parameters.AddWithValue("full_name", app.FullName);
            command.Parameters.AddWithValue("description", app.Description);
            // ссылки на справочники разрешаются отдельным вызовом (см. ResolveDictionaries)
            command.Parameters.AddWithValue("developer_id", (object?)app.DeveloperId ?? DBNull.Value);
            command.Parameters.AddWithValue("category_id", (object?)app.CategoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("rating", app.Rating);
            command.Parameters.AddWithValue("rating_count", app.RatingCount);
            command.Parameters.AddWithValue("price", app.Price);
            command.Parameters.AddWithValue("version", app.Version);
            command.Parameters.AddWithValue("size_mb", app.SizeMB);

            command.Parameters.AddWithValue("age_rating_id", (object?)app.AgeRatingId ?? DBNull.Value);
            command.Parameters.AddWithValue("color", app.Color);
            command.Parameters.AddWithValue("is_featured", app.IsFeatured);
            command.Parameters.AddWithValue("is_in_stock", app.IsInStock);
            command.Parameters.AddWithValue("download_count", app.DownloadCount);
            command.Parameters.AddWithValue(
                "discount_percent",
                app.DiscountPercent.HasValue ? app.DiscountPercent.Value : DBNull.Value
            );
            command.Parameters.AddWithValue("release_date", app.ReleaseDate);
        }

        private static void FillUserParameters(NpgsqlCommand command, User user)
        {
            command.Parameters.AddWithValue("id", user.Id);
            command.Parameters.AddWithValue("login", user.Login);
            command.Parameters.AddWithValue("password_hash", user.PasswordHash);

            command.Parameters.AddWithValue("role", (int)user.Role);

        }

        private static string SerializeTags(List<string> tags) =>
            tags == null || tags.Count == 0 ? string.Empty : string.Join(",", tags);

        private static List<string> ParseTags(string? raw) =>
            string.IsNullOrWhiteSpace(raw)
                ? new List<string>()
                : raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

        private static byte[] CreateSolidLogoBytes(int width, int height, byte r, byte g, byte b)
        {
            var buffer = new byte[width * height * 3];
            for (int i = 0; i < buffer.Length; i += 3)
            {
                buffer[i] = r;
                buffer[i + 1] = g;
                buffer[i + 2] = b;
            }
            return buffer;
        }
    }
}
