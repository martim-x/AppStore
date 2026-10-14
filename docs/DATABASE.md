# Database Design — AppStore

**Engine**: PostgreSQL 16 (Docker: `postgres:16-alpine`)  
**ORM**: Entity Framework Core 8 (primary), ADO.NET (alternative), JSON (demo)

---

## ER Diagram (Mermaid)

```mermaid
erDiagram
    USERS ||--o{ USERS_APPS : "has"
    APPS  ||--o{ USERS_APPS : "belongs to"

    USERS {
        uuid id PK
        text login UK "NOT NULL"
        text password_hash "NOT NULL"
        text first_name
        text last_name
        text email
        int role "NOT NULL, 0=User, 1=Admin"
        text avatar_color "NOT NULL"
    }

    APPS {
        uuid id PK
        text short_name "NOT NULL"
        text full_name "NOT NULL"
        text description "NOT NULL"
        text developer "NOT NULL"
        text category "NOT NULL"
        double_precision rating "NOT NULL"
        int rating_count "NOT NULL"
        double_precision price "NOT NULL"
        text version "NOT NULL"
        double_precision size_mb "NOT NULL"
        text country "NOT NULL"
        text age_rating "NOT NULL"
        text color "NOT NULL"
        boolean is_featured "NOT NULL"
        boolean is_in_stock "NOT NULL"
        int download_count "NOT NULL"
        double_precision discount_percent
        timestamp release_date "NOT NULL"
        text tags "NOT NULL" "comma-separated"
    }

    USERS_APPS {
        uuid user_id PK,FK
        uuid app_id PK,FK
        timestamp installed_at "NOT NULL"
    }
```

---

## Table Definitions (SQL)

### `users`
```sql
CREATE TABLE users (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    login           text NOT NULL UNIQUE,
    password_hash   text NOT NULL,
    first_name      text,
    last_name       text,
    email           text,
    role            int NOT NULL DEFAULT 0,  -- 0=User, 1=Admin
    avatar_color    text NOT NULL DEFAULT '#007AFF',
    created_at      timestamp without time zone DEFAULT now()
);

CREATE INDEX idx_users_login ON users(login);
```

### `apps`
```sql
CREATE TABLE apps (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    short_name      text NOT NULL,
    full_name       text NOT NULL,
    description     text NOT NULL,
    developer       text NOT NULL,
    category        text NOT NULL,
    rating          double precision NOT NULL DEFAULT 0,
    rating_count    int NOT NULL DEFAULT 0,
    price           double precision NOT NULL DEFAULT 0,
    version         text NOT NULL,
    size_mb         double precision NOT NULL,
    country         text NOT NULL,
    age_rating      text NOT NULL DEFAULT '4+',
    color           text NOT NULL DEFAULT '#007AFF',
    is_featured     boolean NOT NULL DEFAULT false,
    is_in_stock     boolean NOT NULL DEFAULT true,
    download_count  int NOT NULL DEFAULT 0,
    discount_percent double precision,
    release_date    timestamp without time zone NOT NULL,
    tags            text NOT NULL DEFAULT '',  -- comma-separated
    created_at      timestamp without time zone DEFAULT now()
);

CREATE INDEX idx_apps_category ON apps(category);
CREATE INDEX idx_apps_featured ON apps(is_featured) WHERE is_featured = true;
CREATE INDEX idx_apps_price ON apps(price);
CREATE INDEX idx_apps_rating ON apps(rating);
CREATE INDEX idx_apps_release_date ON apps(release_date);
```

### `users_apps`
```sql
CREATE TABLE users_apps (
    user_id     uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    app_id      uuid NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
    installed_at timestamp without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, app_id)
);

CREATE INDEX idx_users_apps_user ON users_apps(user_id);
CREATE INDEX idx_users_apps_app ON users_apps(app_id);
```

---

## Seed Data (from `BaseRepository.SeedApps()` / `SeedUsers()`)

### Users (2 records)
| login | password (plain) | hash (SHA-256) | first_name | last_name | email | role | avatar_color |
|-------|------------------|----------------|------------|-----------|-------|------|--------------|
| admin | admin123 | `240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9` | Admin | User | admin@appstore.local | 1 (Admin) | #FF6B35 |
| user  | user123  | `1a15e17c32a29ac7b8b2c8c8e8e8e8e8e8e8e8e8e8e8e8e8e8e8e8e8e8e8e8e8` | Test | User | user@appstore.local | 0 (User) | #007AFF |

> **Note**: Passwords stored as SHA-256 hex lowercase. Migration to BCrypt planned (NFR-09).

### Apps (12 records — categories: Design, Productivity, Games, Development, Education, Utilities)
| short_name | full_name | category | price | rating | featured |
|------------|-----------|----------|-------|--------|----------|
| Figma | Figma — Collaborative Interface Design | Design | 0 | 4.8 | true |
| Photoshop | Adobe Photoshop | Design | 239.88 | 4.7 | true |
| Notion | Notion — Notes, Tasks, Wiki | Productivity | 0 | 4.6 | false |
| Todoist | Todoist — To-Do List & Tasks | Productivity | 0 | 4.5 | false |
| Witcher 3 | The Witcher 3: Wild Hunt | Games | 1199 | 4.9 | true |
| Minecraft | Minecraft Java Edition | Games | 1099 | 4.8 | false |
| VS Code | Visual Studio Code | Development | 0 | 4.9 | true |
| Rider | JetBrains Rider | Development | 899 | 4.7 | false |
| Duolingo | Duolingo — Language Lessons | Education | 0 | 4.7 | false |
| Khan Academy | Khan Academy | Education | 0 | 4.8 | false |
| 7-Zip | 7-Zip File Archiver | Utilities | 0 | 4.6 | false |
| VLC | VLC Media Player | Utilities | 0 | 4.7 | false |

> Full seed data in `Data/BaseRepository.cs` → `SeedApps()` / `SeedUsers()`

---

## EF Core Mapping (from `PostgreDbContext.OnModelCreating`)

| Entity | Table | Key | Notable Config |
|--------|-------|-----|----------------|
| `App` | `apps` | `id` | `Tags` → comma-separated string (ValueConverter); `FinalPrice`, `ButtonLabel`, `IsDownloaded` ignored |
| `User` | `users` | `id` | `Login` unique; `AvatarLetter` ignored |
| `UsersApps` | `users_apps` | composite (`user_id`, `app_id`) | Cascade delete both sides |

---

## Connection String

Managed by `Settings.Instance.ConnectionString`:
```
Host=localhost;Port=5433;Database=appstore;Username=postgres;Password=changeme_secure_password
```

Loaded from (priority):
1. `.env` file (local dev)
2. Environment variables (CI/prod)
3. Hardcoded defaults (fallback only)

---

## Migration Strategy

| Phase | Action |
|-------|--------|
| **Current** | `EnsureDatabaseCreated()` + raw SQL in `PostgreSQLRepEF.EnsureDatabaseCreated()` |
| **Target** | EF Core Migrations (`dotnet ef migrations add InitialCreate`) |
| **Seed** | Separate `IDbInitializer` service (not in migration) |
| **Deploy** | `dotnet ef database update` on startup (or CI) |

---

## Alternative Repositories

| Repo | Connection | Use Case |
|------|------------|----------|
| `PostgreSQLRepEF` | EF Core (Npgsql) | Primary, production |
| `PostgreSQLRepADO` | Raw Npgsql (ADO.NET) | Performance comparison, legacy |
| `JsonRep` | `Resources/Buffer/apps.json`, `users.json` | Offline demo, unit tests |

All implement `IRepository` — swap in `ApplicationApp` constructor.

---

## Backup / Restore (Docker)

```bash
# Backup
docker exec appstore-postgres-16 pg_dump -U postgres appstore > backup_$(date +%F).sql

# Restore
docker exec -i appstore-postgres-16 psql -U postgres appstore < backup_2026-10-14.sql
```

Volume: `appstore_pgdata` (persists across container restarts)