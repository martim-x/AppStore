# Configuration Guide — AppStore

## Overview
All runtime configuration centralized in `Configuration/Settings.cs` (singleton).  
Priority: **Environment Variables** > **`.env` file** > **Hardcoded Defaults**.

---

## Settings.cs API

```csharp
var s = Settings.Instance;

// Database
s.DbHost        // string  (default: "localhost")
s.DbPort        // int     (default: 5433)
s.DbName        // string  (default: "appstore")
s.DbUser        // string  (default: "postgres")
s.DbPassword    // string  (default: "111")
s.ConnectionString  // computed: "Host=...;Port=...;Database=...;Username=...;Password=..."

// Application
s.Environment   // "Development" | "Production" | "Staging"
s.AppName       // "AppStore" (const)
s.AppVersion    // "1.0.0" (const)

// Security
s.PasswordHashAlgorithm  // "SHA256" (current), "BCrypt" (planned)

// UI/UX
s.DefaultLanguage  // "ru" | "en"
s.DefaultTheme     // "Light" | "Dark"
s.DefaultPageSize  // 6
s.MaxPriceFilter   // 1000

// Docker (reference only)
s.DockerImage   // "postgres"
s.DockerTag     // "16-alpine"
s.DockerVolume  // "appstore_pgdata:/var/lib/postgresql/data"

// Validation
s.Validate()  // throws InvalidOperationException if required DB config missing
```

---

## Configuration Sources

### 1. `.env` File (Local Development)
```bash
cp .env.example .env
# Edit values
```

**Format**: `KEY=VALUE` (no spaces around `=`), comments with `#`
```env
DB_HOST=localhost
DB_PORT_FROM=5433
DB_NAME=appstore
DB_ADMIN=postgres
DB_ADMIN_PASS=changeme_secure_password
APP_ENV=Development
DEFAULT_LANGUAGE=ru
DEFAULT_THEME=Light
```

**Location searched** (in order):
1. `AppContext.BaseDirectory/.env` (next to exe)
2. `Directory.GetCurrentDirectory()/.env` (working dir)
3. `../../../.env` (project root from `bin/Debug/net8.0-windows`)

### 2. Environment Variables (CI/CD, Production)
```bash
export DB_HOST=prod-db.internal
export DB_ADMIN_PASS=${SECRET_DB_PASSWORD}
export APP_ENV=Production
```
These **override** `.env` values.

### 3. Hardcoded Defaults (Fallback Only)
Defined in `Settings.cs` constructor. **Never use in production**.

---

## Integration Points (Where to Use `Settings.Instance`)

| File | Before | After |
|------|--------|-------|
| `Data/PostgreSQLRepEF.cs` | `LoadConnectionString()` parses `.env` manually | `Settings.Instance.ConnectionString` |
| `Data/PostgreSQLRepADO.cs` | Hardcoded connection string | `Settings.Instance.ConnectionString` |
| `Services/AuthService.cs` | `SHA256` hardcoded | `Settings.Instance.PasswordHashAlgorithm` (future) |
| `Services/ThemeService.cs` | Default theme in constructor | `Settings.Instance.DefaultTheme` |
| `Services/LocalizationService.cs` | Default lang in constructor | `Settings.Instance.DefaultLanguage` |
| `ViewModels/MainViewModel.cs` | `MaxPrice = 1000`, `_visibleCount = 6` | `Settings.Instance.MaxPriceFilter`, `Settings.Instance.DefaultPageSize` |
| `ApplicationApp.xaml.cs` | Manual ThemeService creation | `Settings.Instance.DefaultTheme` |

---

## Docker Compose Integration

`docker-compose.yml` uses `${VAR:-default}` syntax — reads from shell environment.
```yaml
environment:
  POSTGRES_DB: ${DB_NAME:-appstore}
  POSTGRES_USER: ${DB_ADMIN:-postgres}
  POSTGRES_PASSWORD: ${DB_ADMIN_PASS:-changeme}
ports:
  - "${DB_PORT_FROM:-5433}:${DB_PORT_TO:-5432}"
```

**Workflow**:
```bash
# Local
cp .env.example .env
# Edit .env
docker compose up -d  # reads .env automatically (if in same dir)

# Or export manually
export DB_ADMIN_PASS=secure123
docker compose up -d
```

---

## Adding New Settings

1. Add property to `Settings.cs` with default
2. Add case in `SetProperty(key, value)` switch
3. Document in this file
4. Update `.env.example`
5. Replace all hardcoded usages

---

## Security Notes

- **Never commit `.env`** (in `.gitignore`)
- **Never log `ConnectionString`** (contains password)
- **Rotate passwords** regularly in production
- **Use Docker secrets / GitLab CI variables** for `DB_ADMIN_PASS` in prod
- **Migration to BCrypt** planned (NFR-09) — will add `BCryptWorkFactor` setting

---

## Validation Checklist (Startup)

Run `Settings.Instance.Validate()` early in `ApplicationApp.Main()`:
- [ ] `DbHost` non-empty
- [ ] `DbPort` 1–65535
- [ ] `DbName` non-empty
- [ ] `DbUser` non-empty
- [ ] `DbPassword` non-empty (warn if default "111" or "changeme")

---

## Example: Production `.env` (Template)

```env
# Database (managed by infra)
DB_HOST=postgres.prod.internal
DB_PORT_FROM=5432
DB_NAME=appstore_prod
DB_ADMIN=appstore_app
DB_ADMIN_PASS=__FROM_GITLAB_CI_VARIABLE__

# App
APP_ENV=Production
DEFAULT_LANGUAGE=ru
DEFAULT_THEME=Light
```