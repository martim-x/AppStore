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