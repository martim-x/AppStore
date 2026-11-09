# AppStore Coursework - BACKLOG

## Project Overview
**Application**: AppStore - WPF Desktop Application (C#, .NET 8, MVVM)
**Coursework**: "Программное средство «Магазин приложений»" (Software "App Store")
**Student**: Марушко Тимофей Федорович, ИСиТ 3к 2гр
**Deadline**: 19 декабря 2026 г.
**Discipline**: Объектно-ориентированные технологии программирования и стандарты проектирования

---

## Epic: Coursework Delivery (Milestone: 19.12.2026)

### Sprint 1: Foundation & Documentation (20.09 - 27.09)
- [x] **TASK-001**: Copy project to new repository, clean git history
- [x] **TASK-002**: Create BACKLOG.md with task breakdown
- [ ] **TASK-003**: Create README.md with project description
- [ ] **TASK-004**: Create docs/ARCHITECTURE.md - system architecture
- [ ] **TASK-005**: Create docs/FR_NFR.md - Functional & Non-Functional Requirements
- [ ] **TASK-006**: Create docs/UML/ - Use Case, Class, Sequence diagrams (PlantUML/Mermaid)
- [ ] **TASK-007**: Create docs/PROJECT_PLAN.md - calendar plan per assignment

### Sprint 2: Architecture & Design (27.09 - 06.10)
- [ ] **TASK-008**: Analyze existing codebase - document current architecture
- [ ] **TASK-009**: Create Settings.cs - centralized configuration class
- [ ] **TASK-010**: Refactor configuration - move all hardcoded values to appsettings.json
- [ ] **TASK-011**: Fix Docker Compose - pin exact PostgreSQL image version (no latest)
- [ ] **TASK-012**: Create .env.example template
- [ ] **TASK-013**: Document design patterns used (MVVM, Repository, Command, Strategy, Factory)

### Sprint 3: Database Design & Implementation (06.10 - 20.10)
- [ ] **TASK-014**: Create docs/DATABASE.md - ER diagram, schema documentation
- [ ] **TASK-015**: Review EF Core migrations strategy
- [ ] **TASK-016**: Add database seeding with realistic test data
- [ ] **TASK-017**: Implement connection resilience (retry policy)
- [ ] **TASK-018**: Add database health check endpoint

### Sprint 4: Core Development (20.10 - 14.11)
- [ ] **TASK-019**: Admin features - Add/Edit/Delete apps with validation
- [ ] **TASK-020**: Admin features - Restore defaults with confirmation
- [ ] **TASK-021**: Client features - Catalog with "Pick of the Day", categories, full list
- [ ] **TASK-022**: Client features - App detail view with all metadata
- [ ] **TASK-023**: Client features - Search by name, developer, category, tags
- [ ] **TASK-024**: Client features - Filter by price, rating, tags, installed status
- [ ] **TASK-025**: Client features - Sort by name, price, rating, release date
- [ ] **TASK-026**: Client features - Install/Uninstall with Undo/Redo (Command pattern)
- [ ] **TASK-027**: Client features - Profile management (name, email, password change)
- [ ] **TASK-028**: Client features - Language switch (RU/EN) and Theme switch (Light/Dark)
- [ ] **TASK-029**: Authentication - Login/Logout, role-based UI (Admin vs User)
- [ ] **TASK-030**: Password hashing - SHA-256 (upgrade to BCrypt/Argon2 for production)

### Sprint 5: Testing (14.11 - 28.11)
- [ ] **TASK-031**: Set up xUnit test project structure
- [ ] **TASK-032**: Unit tests - Repository layer (JSON, EF, ADO) with in-memory/testcontainers
- [ ] **TASK-033**: Unit tests - Services (AuthService, ThemeService, LocalizationService)
- [ ] **TASK-034**: Unit tests - ViewModels (MainViewModel, AddEditAppViewModel, ProfileViewModel)
- [ ] **TASK-035**: Unit tests - Commands (RelayCommand, Undo/Redo actions)
- [ ] **TASK-036**: Unit tests - Converters
- [ ] **TASK-037**: Integration tests - Database operations (PostgreSQL 16 service container)
- [ ] **TASK-038**: Integration tests - Full authentication flow
- [ ] **TASK-039**: Integration tests - App CRUD operations
- [ ] **TASK-040**: Integration tests - Install/Uninstall with Undo/Redo
- [ ] **TASK-041**: Code coverage target: ≥80% for business logic

### Sprint 6: Documentation & Defense Prep (28.11 - 10.12)
- [ ] **TASK-042**: Create report.json for bsstu-report builder
- [ ] **TASK-043**: Generate docx + pdf via bsstu-report
- [ ] **TASK-044**: Create user manual (Руководство пользователя)
- [ ] **TASK-045**: Prepare code listings for appendix
- [ ] **TASK-046**: Create presentation slides for defense
- [ ] **TASK-047**: Verify all СТП БГТУ 001-2019 formatting requirements

### Sprint 7: Final Polish & GitLab Push (10.12 - 19.12)
- [ ] **TASK-048**: Push to GitLab, verify CI/CD
- [ ] **TASK-049**: GitLab Duo code review - address findings
- [ ] **TASK-050**: Final verification - build, test, run
- [ ] **TASK-051**: Package installation artifacts
- [ ] **TASK-052**: Submit to instructor

---

## Ревью пояснительной записки — 26.10.2026

Источник: комментарии руководителя в `~/Downloads/Marushko_T_F_ISiT_3k_2gr.docx`
(11 комментариев Word, автор «Пользователь Windows», 26.10.2026 14:54–15:04).
Замечания обязательны к устранению до повторной сдачи записки.

### P0 — блокируют принятие записки

- [ ] **REVIEW-001** (п. 2.2, рисунок 2.1) — диаграмма вариантов использования не читается при печати: мелкий шрифт, не соответствует заданию. Перерисовать в draw.io: чёрно-белая, ортогональные связи (`edgeStyle=orthogonalEdgeStyle;rounded=0`), акторы и прецеденты строго по заданию, кегль подписи ≥ 12 pt при ширине композиции ≤ 1100 ед.
- [ ] **REVIEW-002** (п. 2.3, рисунок 2.2) — на диаграмме классов разные типы стрелок без пояснения; руководитель сомневается в их корректности. Привести нотацию UML к однозначной: наследование — `endArrow=block;endFill=0`, реализация интерфейса — то же + `dashed=1`, зависимость — `endArrow=open;dashed=1`, ассоциация — `endArrow=none`; в тексте расшифровать типы связей.
- [ ] **REVIEW-003** (п. 2.4, рисунок 2.3) — диаграмма последовательности показывает только запуск и вход. Добавить ключевые сценарии: установка/удаление приложения с отменой действия (Command/Undo), поиск и фильтрация каталога, добавление/редактирование/удаление приложения администратором, смена профиля и пароля.
- [ ] **REVIEW-004** (п. 2.5, рисунок 2.4) — схема базы данных «плохо почти всё» (комментарий «нет»). Перерисовать ERD заново в draw.io: чёрно-белая, ортогональная, читаемая, с PK/FK и кардинальностями для `users`, `apps`, `users_apps`; проверить печать.
- [ ] **REVIEW-005** (п. 1.1 «Описание предметной области») — текст описывает уже реализованное («реализованы установка и удаление…», «разграничение доступа реализовано…»), хотя это постановка задачи. Переписать в терминах предметной области и требований; описание реализации перенести в раздел 3.
- [ ] **REVIEW-006** (п. 1.2 «Обзор аналогов») — два абзаца не являются обзором. Развернуть: по абзацу на каждый аналог (Microsoft Store, Steam, Google Play, App Store), сравнительная таблица функций, вывод; добавить ссылки на источники аналогов (комментарий «Ссылки на аналоги»).
- [ ] **REVIEW-007** (п. 1.3–1.4) — требования должны быть списаны с задания и сформулированы как требования («средство должно обеспечивать…»), а не как отчёт о выполнении. Убрать утверждение «Все нефункциональные требования выполнены в полном объёме» из раздела требований.

### P1 — оформление и терминология

- [ ] **REVIEW-008** (п. 2.2) — использовать корректный термин UML «прецеденты» (комментарий «Как они называются? Прецеденты?»), пояснить акторов и состав прецедентов.
- [ ] **REVIEW-009** (весь документ) — не оставлять пустое место в конце страницы (комментарий «здесь и везде»): проверить разрывы страниц, привязку рисунков к подписям, `keep with next` для заголовков.
- [ ] **REVIEW-010** (введение, перечень задач) — пунктуация перечисления: последний пункт заканчивается точкой, а не точкой с запятой (комментарий «Точка?»).
- [ ] **REVIEW-011** (рисунки 2.1–2.4) — единый читаемый шрифт на всех схемах (замечание «шрифт» повторяется): один кегль и гарнитура, без мелких подписей; проверить в PDF при 100 % и в чёрно-белой печати.

---

## Technical Debt / Improvements (Post-Coursework)
- [ ] **TECH-001**: Migrate to .NET 9 LTS when available
- [ ] **TECH-002**: Replace SHA-256 with BCrypt/Argon2 for password hashing
- [ ] **TECH-003**: Add structured logging (Serilog)
- [ ] **TECH-004**: Implement proper dependency injection (Microsoft.Extensions.DI)
- [ ] **TECH-005**: Add API layer for future web/mobile clients
- [ ] **TECH-006**: Implement proper migration system (EF Core Migrations)
- [ ] **TECH-007**: Add automated UI tests (Playwright/Avalonia)
- [ ] **TECH-008**: Performance optimization - virtualized lists, pagination
- [ ] **TECH-009**: Accessibility improvements (WCAG 2.1 AA)

---

## Design Patterns Identified (Requirement: 2+ patterns)
1. **MVVM** - Model-View-ViewModel (primary architecture)
2. **Repository** - IRepository with multiple implementations (EF, ADO, JSON)
3. **Command** - RelayCommand, Undo/Redo actions (IUndoRedoAction)
4. **Strategy** - Repository implementations interchangeable at runtime
5. **Factory** - ViewModel/View creation in ApplicationApp
6. **Observer** - INotifyPropertyChanged in ViewModelBase
7. **Singleton** - ThemeService, LocalizationService (app-scoped)

---

## Definition of Done (per task)
- [ ] Code compiles without warnings
- [ ] Unit tests pass (≥80% coverage for new code)
- [ ] Integration tests pass
- [ ] Documentation updated
- [ ] No hardcoded configuration values
- [ ] Follows MVVM pattern strictly
- [ ] Code reviewed (self + GitLab Duo)

---

## Правки документации — 09.11.2026

### Личная задача автора
- [ ] **AUTHOR-001**: ERD, рисунок 2.4 — перерисовать самому по методичке: три секции таблицы, колонка спецификаторов PK/FK, подчёркнутые ключи, кратности, ч/б. Схему в `docs/UML/erd.drawio` я не трогал.

### Ждём релиз билдера
- [x] **DEBT-001**: выборочный курсив реализован в BstuReportBuilder (`--references-latin-italic false`); записка пересобрана, список источников прямой.

### Обнаружено при анализе
- [x] **DEBT-002**: `scripts/report/build_report_json.py` устарел — генерирует 9 источников и не содержит таблиц раздела 1; запуск перезапишет актуальный `docs/report.json`. Скрипт переведён в режим защиты: без `--force` завершается с кодом 2 и объяснением.
- [x] Таблица 1.1 (сопоставление аналогов) удалена, абзацы до/после переписаны без ссылки на неё.
- [x] Таблица 1.1 (бывшая 1.2) «Функциональные требования» — столбец «Обозначение» (FR-01…FR-17) удалён.
- [x] Список источников переставлен под порядок первого упоминания (xUnit — [6], PostgreSQL — [7]).
- [x] Рисунки руководства пользователя уменьшены до 7,5/6/6,5 см.
- [x] Схемы 2.1, 2.2, 2.3 пересобраны генератором `scripts/diagrams/make_diagrams.py` (ч/б, ≤1100 ед. по ширине).

---

## Остаток работ — 09.11.2026 (постановка автора)

### Диаграммы (приоритет: важно)
- [ ] **UML-001**: переделать полностью все 4 схемы по образцам из `Требования_к_курсовому поООП, ООТСП,СТПМУ_2022-1.pdf` (классы, варианты использования, последовательность, ER). Текущая версия — компромисс: ч/б, ортогонально, но не повторяет образец один-в-один. Если соответствие образцу не достигается — фиксировать как тех-долг с описанием расхождения.
- [ ] **UML-002** (автор): ERD, рисунок 2.4 — доделывает сам.

### Приложение
- [x] **APP-001**: перекомпоновать интерфейс — переключатель языка сейчас в боковом меню, должен быть в настройках; проверить и переставить остальные элементы по логике «настройки отдельно от навигации».
- [x] **APP-002**: регистрация пользователей. Сейчас в системе вшитый администратор; нужна самостоятельная регистрация обычного пользователя и изоляция данных: у каждого пользователя свои заявки/установки/профиль, чужие данные недоступны.
- [x] **APP-003**: оценки приложений. Пользователь ставит оценку (1–5), рейтинг приложения формируется как агрегат оценок (пример: 4, 4, 5, 5, 4 → 4,4), отображается в карточке и в каталоге; хранить оценки в БД (таблица `app_ratings` + агрегат).

### Тех-долг по замечанию автора (09.11.2026)
- [ ] **UML-003**: переделать три схемы: вариантов использования, классов и последовательностей. ERD (рисунок 2.4) остаётся за автором, см. UML-002.
- [ ] **DOC-001**: вставить официальный титульный лист в записку; сейчас титул формируется билдером из шаблона.

### Качество и проверки
- [x] **QA-001**: тесты на новую функциональность (регистрация, изоляция данных, оценки) + прогон существующего набора; проверка работы приложения после перекомпоновки UI.
- [ ] **TOOL-001** (тех-долг, ограничение инструментов): используемая модель (deepseek) не анализирует изображения — «компьютерное зрение» сводится к разбору текста/структуры, поэтому визуальную проверку схем (наложения, читаемость на печати) приходится делать вручную по PNG; автоматическая проверка вёрстки схем недоступна.

### Журнал выполненных работ (09.11.2026)
- [x] Удалена таблица 1.1 (сопоставление аналогов), абзацы переписаны.
- [x] Из таблицы функциональных требований убран столбец «Обозначение».
- [x] Список источников: порядок первого упоминания `[1]…[7]`, курсив убран (`--references-latin-italic false`).
- [x] Рисунки руководства пользователя уменьшены (7,5 / 6 / 6,5 см).
- [x] «Вывод по разделу» → «Выводы по разделу» (требование билдера).
- [x] Схемы 2.1–2.3 пересобраны: ч/б, ортогональные связи, ширина ≤1100 ед., шрифт 8–9 pt на печати.
- [x] Сборка: 35 страниц, `--check` без ошибок, `--fit-figures` не применяется (давал пустоты 82–102 мм).
- [x] Таблица 2.2 не разрывается (принудительный разрыв перед таблицей): фактический разрыв приходился на 2-ю строку, а пагинатор делит только с 3-й; иначе LibreOffice рвал строку без подписи.
- [x] «Продолжение таблицы 1.1» формируется корректно (17 строк, две части, повтор шапки).
---

## Журнал — 09.11.2026, вечер (APP-001…003 закрыты)

### Сделано
- **APP-002**: `AuthService.Register` (логин 3–32 символа, пароль ≥ 8, уникальность без учёта регистра, BCrypt, роль User, авто-вход), переключение окна входа в режим регистрации, ключи локализации RU/EN. Изоляция данных подтверждена: `users_apps` и `user_profiles` фильтруются по текущему пользователю.
- **APP-003**: таблица `app_ratings` (составной PK, CHECK 1–5), миграция `AddAppRatings`, реализации в EF/ADO/Json, `RatingAggregator` (среднее по накопленной сумме), блок «Ваша оценка» в карточке, рейтинг и число оценок в строках и плитках каталога. Звёзды заливаются по текущей оценке пользователя и меняются сразу после нажатия: `RatingStarStateConverter` вызывает `RatingAggregator.StarFor`, повторная оценка заменяет прежнюю и не увеличивает число оценок.
- **APP-001**: переключатель языка убран из бокового меню и перенесён в настройки профиля; попап позиционируется по фактической высоте и не уходит за нижний край экрана.
- **QA-001**: 7 unit-тестов (регистрация, агрегат оценок) и 5 интеграционных (оценки, регистрация, изоляция). Интеграционные тесты больше не оставляют мусор в каталоге.
- **DEBT-002**: устаревший генератор `build_report_json.py` защищён флагом `--force`.
- Записка дополнена подразделами 3.4 и 3.5 (регистрация, оценки), строками FR и `app_ratings` в таблице состава БД, листингом А.11.

### Доказательства
- `dotnet test Project.sln`: 56 unit + 15 integration, 0 failed.
- Миграция применена к контейнеру `appstore-postgres-16` (порт 5433).
- Ручная проверка: регистрация `smoke_test`, оценка 5★ для Visual Studio Code — в БД `app_ratings` value = 5, `apps.rating_count` = 890001; переключение языка из настроек профиля переводит интерфейс на русский.
- Отчёт: 39 страниц, `app build --check` без ошибок, 15 рисунков, 7 таблиц.

### Осталось
- **UML-002** (ERD, рисунок 2.4) — за автором; таблица `app_ratings` на схеме пока не отражена.
- **UML-001** — соответствие схем образцу методички (тех-долг).
- **REVIEW-003** (сценарии последовательности: установка/удаление, поиск, админ-CRUD, профиль), **REVIEW-006** (четыре аналога вместо двух).

### Скриншоты руководства (09.11.2026, вечер)
- **MANUAL-001**: снят полный цикл работы для раздела 5: вход, каталог, поиск, карточка, оценка, установка, профиль, добавление приложения, регистрация. Файлы: `docs/screenshots/manual/*.png`.
- Съёмка через `scripts/shots/shot.ps1`: кадр берётся по окну процесса, оверлей агента скрывается (`ShowWindow(SW_HIDE)` для окон `Cua.AgentCursorOverlay*`), курсор уводится в угол. Попапы снимаются переключателями `-AllWindows` и `-SmallWindow`.
- Записка пересобрана: раздел 5 дополнен подразделами 5.8–5.10, 19 рисунков, 43 страницы, `--check` без ошибок.
