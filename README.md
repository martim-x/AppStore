# AppStore — WPF Desktop Application

> Курсовой проект по дисциплине **«Объектно-ориентированные технологии программирования и стандарты проектирования»**  
> БГТУ, Факультет ИТ, Кафедра ПИ, Группа 2, Студент: Марушко Т.Ф.

---

## Описание проекта

**AppStore** — настольное приложение (WPF, .NET 8, Windows) для управления каталогом приложений. Реализует роли **Администратор** и **Клиент** с разграничением функций.

### Основные возможности

| Роль | Функции |
|------|---------|
| **Администратор** | Вход в систему, просмотр каталога с поиском/фильтрацией/сортировкой, установка/удаление приложений, управление профилем, **добавление/редактирование/удаление приложений**, восстановление исходного каталога, админ-функции только для роли Admin |
| **Клиент** | Вход в систему, просмотр каталога («Выбор дня», категории, полный список), детальная карточка приложения, поиск, фильтрация (цена, рейтинг, теги, установленные), сортировка (название, цена, рейтинг, дата), установка/удаление с **Undo/Redo**, профиль (имя, email, смена пароля), переключение языка (RU/EN) и темы (Light/Dark), выход |
| **Регистрация** | Самостоятельное создание учётной записи с уникальным логином и паролем (BCrypt, не короче 8 символов); данные пользователя изолированы: установленные приложения и профиль доступны только владельцу |
| **Оценки** | Выставление и изменение оценки приложения от 1 до 5; рейтинг приложения формируется как агрегат оценок и обновляется в карточке и списке |

---

## Технологический стек

| Компонент | Технология |
|-----------|------------|
| **Язык** | C# 12 (.NET 8) |
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **Архитектура** | MVVM (Model-View-ViewModel) |
| **БД** | PostgreSQL 16 (Docker) |
| **ORM** | Entity Framework Core 8 + Npgsql |
| **Альтернативы БД** | ADO.NET (Npgsql), JSON (файловое хранилище) |
| **DI/Конфигурация** | Ручная (планируется Microsoft.Extensions.DI) |
| **Тестирование** | xUnit, Moq, PostgreSQL 16 service container |
| **Сборка** | MSBuild / `dotnet build` |
| **Документация** | bsstu-report (ГОСТ/СТП БГТУ 001-2019) |

---

## Архитектура (схема)

```
┌─────────────────────────────────────────────────────────────┐
│                      ApplicationApp                          │
│  (Bootstrap: Theme, Cursor, Login → Catalog ↔ Login loop)   │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│   Views       │    │  ViewModels   │    │  Services     │
│  (XAML + CS)  │◄───│  (INPC, CMD)  │───►│  (Auth, Theme,│
│  Login, Main, │    │  MainVM,      │    │  Localization,│
│  Detail, Edit │    │  AddEditVM,   │    │  UndoRedo)    │
│  Profile      │    │  ProfileVM    │    └───────┬───────┘
└───────────────┘    └───────────────┘            │
                                                  ▼
                                         ┌───────────────┐
                                         │  Repository   │
                                         │  (Interface)  │
                                         └───────┬───────┘
                                                 │
                    ┌────────────────────────────┼────────────────────────────┐
                    ▼                            ▼                            ▼
            ┌───────────────┐            ┌───────────────┐            ┌───────────────┐
            │ PostgreSQLRepEF│            │PostgreSQLRepADO│            │   JsonRep     │
            │  (EF Core 8)  │            │   (ADO.NET)   │            │  (Files)      │
            └───────────────┘            └───────────────┘            └───────────────┘
```

---

## Запуск проекта

### Предварительные требования
- .NET 8 SDK
- Docker Desktop (для PostgreSQL)
- Visual Studio 2022 / Rider / VS Code с C# Dev Kit

### 1. Клонирование и настройка
```bash
git clone <repo-url>
cd AppStore-Coursework
cp .env.example .env   # отредактировать при необходимости
```

### 2. Запуск базы данных (Docker)
```bash
docker compose up -d
# PostgreSQL будет доступен на localhost:5433
```

### 3. Сборка и запуск
```bash
dotnet restore
dotnet build
dotnet run --project Project.csproj
```

### 4. Тестовые учётные записи (после первого запуска — сиды)
| Роль | Логин | Пароль |
|------|-------|--------|
| Admin | `admin` | `admin123` |
| User  | `user`  | `user123`  |

---

## Структура репозитория

```
AppStore-Coursework/
├── ApplicationApp.xaml(.cs)      # Точка входа, навигация Login↔Catalog
├── Commands/                     # RelayCommand, AppCommands (Undo/Redo)
├── Converters/                   # Value converters для XAML binding
├── Data/
│   ├── Models.cs                 # App, User, UsersApps (POCO + IApp/IUser)
│   ├── IRepository.cs            # Интерфейс репозитория
│   ├── BaseRepository.cs         # Базовый класс с Seed-данными
│   ├── PostgreSQLRepEF.cs        # EF Core реализация
│   ├── PostgreSQLRepADO.cs       # ADO.NET реализация
│   └── JsonRep.cs                # JSON-файловая реализация
├── Services/
│   ├── AuthService.cs            # Аутентификация, пароли (SHA-256)
│   ├── ThemeService.cs           # Light/Dark темы
│   ├── LocalizationService.cs    # RU/EN локализация
│   └── UndoRedoService.cs        # Стек команд Undo/Redo
├── ViewModels/
│   ├── ViewModelBase.cs          # INotifyPropertyChanged базовый
│   ├── MainViewModel.cs          # Главный VM (каталог, фильтры, навигация)
│   ├── AddEditAppViewModel.cs    # Диалог добавления/редактирования
│   └── ProfileViewModel.cs       # Профиль пользователя
├── Views/
│   ├── MainWindow.xaml(.cs)      # Главное окно (каталог)
│   ├── LoginWindow.xaml(.cs)     # Окно входа
│   ├── AppDetailView.xaml(.cs)   # Детальная карточка приложения
│   ├── AddEditWindow.xaml(.cs)   # Диалог добавления/редактирования
│   ├── ProfilePopup.xaml(.cs)    # Попап профиля
│   └── ...                       # Остальные View
├── Resources/
│   ├── Styles/                   # LightAppTheme, DarkAppTheme
│   ├── Localization/             # en.xaml, ru.xaml
│   ├── Icons/, Cursors/, Buffer/
├── docker-compose.yml            # PostgreSQL 16 (версия зафиксирована)
├── .env.example                  # Шаблон конфигурации БД
├── docs/                         # Документация (ARCHITECTURE, FR_NFR, DATABASE, etc.)
├── tests/
│   ├── unit/                     # xUnit юнит-тесты
│   └── integration/              # Интеграционные тесты (PostgreSQL 16 service container)
├── BACKLOG.md                    # План работ и задачи
├── PROJECT_PLAN.md               # Календарный план (по заданию)
└── README.md                     # Этот файл
```

---

## Паттерны проектирования (требование: ≥2)

1. **MVVM** — разделение View (XAML), ViewModel (логика представления), Model (Data/Services)
2. **Repository** — `IRepository` с тремя реализациями (EF, ADO, JSON), переключаемыми в runtime
3. **Command** — `RelayCommand` + `IUndoRedoAction` (GetAction/OpenAction) для Undo/Redo
4. **Strategy** — репозитории как стратегии хранения данных
5. **Factory** — создание View/ViewModel в `ApplicationApp.OpenCatalog/ShowLogin`
6. **Observer** — `INotifyPropertyChanged` во `ViewModelBase`
7. **Singleton (app-scoped)** — `ThemeService`, `LocalizationService` живут весь цикл приложения

---

## Конфигурация

Все настройки вынесены в **`Settings.cs`** (см. `docs/CONFIGURATION.md`) и `.env` файл.

### Основные параметры (.env)
```env
# Database
DB_IMAGE=postgres
DB_VERSION=16-alpine
DB_NAME=appstore
DB_ADMIN=postgres
DB_ADMIN_PASS=secure_password_change_me
DB_PORT_FROM=5433
DB_PORT_TO=5432
DB_VOLUME=appstore_pgdata:/var/lib/postgresql/data

# App
APP_ENV=Development
```

> **Важно**: В продакшене пароль БД **обязательно** менять. Используйте секреты Docker / переменные окружения CI.

---

## Тестирование

```bash
# Все тесты
dotnet test

# Только юнит-тесты
dotnet test tests/unit

# Только интеграционные (требует Docker)
dotnet test tests/integration

# С покрытием
dotnet test --collect:"XPlat Code Coverage"
```

**Цель покрытия**: ≥80% для бизнес-логики (Services, ViewModels, Repository).

---

## Документация

| Файл | Описание |
|------|----------|
| `docs/ARCHITECTURE.md` | Архитектурное решение, диаграммы |
| `docs/FR_NFR.md` | Функциональные и нефункциональные требования |
| `docs/DATABASE.md` | ER-диаграмма, схема БД, миграции |
| `docs/CONFIGURATION.md` | Settings.cs, .env, appsettings.json |
| `docs/UML/` | Use Case, Class, Sequence диаграммы (Mermaid) |
| `docs/PROJECT_PLAN.md` | Календарный план выполнения |
| `BACKLOG.md` | Детальный бэклог задач |

---

## Генерация отчёта (bsstu-report)

```bash
cd ~/Documents/VisualStudioCodeRepos/BstuReportBuilder
/opt/homebrew/bin/python3 -m app build report.json -o ../AppStore-Coursework/Курсовая_Марушко.docx --pdf --check
```

---

## Лицензия

Образовательный проект. Использование в учебных целях.

---

## Контакты

- **Студент**: Марушко Тимофей Федорович
- **Группа**: ИСиТ 3к 2гр
- **Руководитель**: Савельева М.Г.
- **Email**: (уточнить у пользователя)