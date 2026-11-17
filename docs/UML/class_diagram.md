# Диаграмма классов AppStore

## Источник и состав

- Редактируемый источник: `class_diagram.drawio`.
- Страница «class» сохраняет координаты 30 прежних блоков и добавляет `BaseRepository` в свободное место. На ней 31 блок класса и 34 связи.
- Страница «Дополнительные классы» содержит 45 свободных блоков. Это 44 подтверждённых типа из текущего кода и внешний интерфейс `ICommand`. Блоки намеренно оставлены без связей и без группировки, чтобы владелец мог сам разместить их и дополнить содержимое.
- PNG-экспорты страниц: `class_diagram.png` и `class_diagram_extra.png`.

## Связи основной страницы

- Обобщение, сплошная линия с пустым треугольником: `MainViewModel`, `ProfileViewModel` и `AddEditAppViewModel` наследуют `ViewModelBase`; `PostgreSQLRepEF`, `PostgreSQLRepADO` и `JsonRep` наследуют `BaseRepository`.
- Реализация, пунктирная линия с пустым треугольником: `BaseRepository` реализует `IRepository`; `AuthService` реализует `IAuthService`; `BCryptHasher` и `SHA256Hasher` реализуют `IPasswordHasher`; реализации тем, локализации и отмены действий связаны с соответствующими интерфейсами.
- Направленная ассоциация, сплошная линия с открытым наконечником: окна связаны с моделями представления; `MainViewModel` связан с репозиторием, службами аутентификации, темы, локализации и отмены действий; `ProfileViewModel` связан с аутентификацией и темой; `AddEditAppViewModel` связан с `IRepository`; `AuthService` связан с репозиторием, хешером и текущим `User`.
- Ассоциации `User`–`UsersApps` и `App`–`UsersApps` имеют кратности `1` и `0..*`; открытая стрелка показывает направление связи к `UsersApps`. Смысл каждой кратности расположен у соответствующего блока.
- Зависимость, пунктирная линия с открытым наконечником: `MainViewModel` использует `RelayCommand`; `CompositionRoot` зависит от `AuthService`, `IRepository` и `DatabaseHealthCheckService`.
- Композиция и агрегация не нарисованы. В коде не подтверждено владение жизненным циклом частей, необходимое для ромбов.

Нотация шести типов связей сверена с источником Visual Paradigm, приведённым под номером [9] в `docs/report.json`.

## Свободные блоки дополнительной страницы

`ApplicationApp`, `AppCommands`, `RelayCommand<T>`, `BoolToVisibilityConverter`, `InverseBoolToVisibilityConverter`, `NullToVisibilityConverter`, `PriceConverter`, `DownloadCountConverter`, `RatingToStarsConverter`, `SizeConverter`, `StringToColorBrushConverter`, `DiscountVisibilityConverter`, `RoleNameConverter`, `ButtonLabelConverter`, `LocalizedDescriptionConverter`, `RatingStarStateConverter`, `CategoryConverter`, `CategoryItem`, `AppDetailView`, `AppRowView`, `ConfirmDialog`, `FeaturedBannerView`, `Settings`, `AppOrdering`, `Country`, `Developer`, `CatalogCategory`, `AgeRating`, `Tag`, `AppTag`, `UserRole`, `IApp`, `IUser`, `IUsersApps`, `UserProfile`, `AppRating`, `PostgreDbContext`, `GetAction`, `OpenAction`, `HealthCheckResult`, `IHealthCheckService`, `IUndoRedoAction`, `RatingAggregator`, `RegistrationResult`, `ICommand`
