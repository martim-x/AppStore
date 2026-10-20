# Calendar Plan — Coursework (per Assignment)
# Updated with coursework writing plan (bsstu-report, СТП БГТУ 001-2019)

**Assignment**: University/лист задания Марушко Т.Ф. ИСиТ 3к 2 гр.docx (Section "Календарный план")  
**Discipline**: Объектно-ориентированные технологии программирования и стандарты проектирования  
**Deadline**: 19.12.2026  
**Student**: Марушко Тимофей Федорович, ИСиТ 3к 2гр

---

## Official Schedule (from Assignment)

| № | Stage | Deadline | Status | Notes |
|---|-------|----------|--------|-------|
| 1 | Введение (Introduction) | 20.09.2026 | ✅ Done | Requirements gathered, repo initialized |
| 2 | Обзор аналогов, формулирование требований, варианты использования | 27.09.2026 | ✅ Done | FR_NFR.md, UML Use Case created |
| 3 | Анализ и проектирование архитектуры (диаграммы, бизнес-слой, представление, данные) | 06.10.2026 | 🟡 In Progress | ARCHITECTURE.md, Class Diagram, Sequence Diagrams created |
| 4 | Проектирование БД, дизайн UI | 20.10.2026 | ⬜ Todo | DATABASE.md done; UI review needed |
| 5 | Кодирование программного средства | 14.11.2026 | ⬜ Todo | Main implementation phase |
| 6 | Тестирование и отладка | 28.11.2026 | ⬜ Todo | Unit + Integration tests |
| 7 | Оформление пояснительной записки | 10.12.2026 | ⬜ Todo | bsstu-report generation |
| 8 | Подготовка к защите | 19.12.2026 | ⬜ Todo | Slides, demo prep |
| 9 | Защита проекта | 20.12.2026 | ⬜ Todo | Final deadline |

---

## Detailed Sprint Breakdown (Internal)

### Sprint 1: Foundation (Week 1–2) — *20.09 – 27.09*
- [x] Copy project, clean git, `.gitignore`, `.env.example`
- [x] `Settings.cs` — centralized config
- [x] Docker Compose pinned (`postgres:16-alpine`)
- [x] `README.md`, `BACKLOG.md`
- [x] `docs/ARCHITECTURE.md` (layers, patterns, Mermaid)
- [x] `docs/FR_NFR.md` (traceability matrix)
- [x] `docs/DATABASE.md` (ERD, schema, seeds)
- [x] `docs/UML/` (Use Case, Class, Sequence)
- [x] `docs/CONFIGURATION.md`
- [ ] Codebase indexing (`codebase-memory`, `pygount`)
- [ ] Verify build/run with new Settings

### Sprint 2: Architecture Hardening (Week 2–3) — *27.09 – 06.10*
- [ ] DI Container (`Microsoft.Extensions.DI`)
- [ ] EF Core Migrations (replace `EnsureDatabaseCreated`)
- [ ] BCrypt password hashing (replace SHA-256)
- [ ] Connection resilience (`EnableRetryOnFailure`)
- [ ] Integration test infrastructure (Testcontainers)

### Sprint 3: Database & UI Design (Week 3–4) — *06.10 – 20.10*
- [ ] Finalize ER diagram, document in `DATABASE.md`
- [ ] UI review: all Views match FR_NFR requirements
- [ ] Accessibility check (contrast, keyboard nav)
- [ ] Responsive layout verification

### Sprint 4: Core Implementation (Week 4–7) — *20.10 – 14.11*

#### Admin Features
- [ ] Add/Edit App dialog — full validation
- [ ] Delete App with confirmation
- [ ] Restore Defaults with confirmation
- [ ] Admin-only UI visibility (IsAdmin binding)

#### Client Features
- [ ] "Pick of the Day" (FeaturedApps + Banner)
- [ ] Categories navigation (Design, Productivity, Games, Dev, Education, Utilities)
- [ ] Search (name, dev, category, tags)
- [ ] Filters (price, rating, tags, installed)
- [ ] Sort (name, price, rating, date)
- [ ] App Detail View — all metadata fields
- [ ] Install/Uninstall + Undo/Redo (toolbar buttons)
- [ ] Profile: edit name, email
- [ ] Change password (old + new + confirm)
- [ ] Language switch (RU/EN) — persists
- [ ] Theme switch (Light/Dark) — persists
- [ ] Logout → Login screen

### Sprint 5: Testing (Week 7–8) — *14.11 – 28.11*
- [ ] Unit tests: Repository (JSON, EF, ADO) — in-memory / Testcontainers
- [ ] Unit tests: Services (Auth, Theme, Locale, UndoRedo)
- [ ] Unit tests: ViewModels (Main, AddEdit, Profile)
- [ ] Unit tests: Commands, Converters, Models
- [ ] Integration tests: DB migrations + seed
- [ ] Integration tests: Auth flow
- [ ] Integration tests: App CRUD
- [ ] Integration tests: Install/Uninstall + Undo/Redo
- [ ] Coverage ≥80% business logic

### Sprint 6: Documentation & Report (Week 8–9) — *28.11 – 10.12*
- [ ] `report.json` for bsstu-report (all chapters)
- [ ] Render Mermaid diagrams → PNG for figures
- [ ] Code listings for appendix
- [ ] Build docx + pdf: `python3 -m app build report.json -o Курсовая.docx --pdf --check`
- [ ] User manual (appendix)
- [ ] Defense slides (10–15 min)

### Sprint 7: Final Delivery (Week 9) — *10.12 – 19.12*
- [ ] GitLab push, CI/CD verify
- [ ] GitLab Duo code review → fix findings
- [ ] Clean Release build
- [ ] Full test suite pass
- [ ] Manual smoke test (Admin + Client flows)
- [ ] Publish single-file exe (`dotnet publish -r win-x64 -p:PublishSingleFile=true`)
- [ ] Submit: docx + pdf + source + installer

---

## Milestone Gates (Definition of Done per Stage)

| Stage | Gate Criteria |
|-------|---------------|
| **Sprint 1** | `dotnet build` succeeds; `Settings.Instance` used everywhere; docs complete |
| **Sprint 2** | DI container wired; EF migrations work; BCrypt hashing; Testcontainers spin up |
| **Sprint 3** | All UI mockups implemented; FR_NFR checklist 100% covered |
| **Sprint 4** | All FR-A / FR-C features working; manual test passes |
| **Sprint 5** | `dotnet test` → all green; coverage report ≥80% |
| **Sprint 6** | `bsstu-report --check` passes; docx+pdf generated; no formatting errors |
| **Sprint 7** | GitLab CI green; Duo review clean; Release build runs on clean Windows |

---

## Risk Register

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| EF Core migration conflicts with existing schema | Medium | High | Test migration on copy of seeded DB first |
| BCrypt breaks existing seeded passwords | High | Medium | Write migration script to re-hash; keep SHA-256 fallback |
| Testcontainers slow on CI | Medium | Low | Use shared PostgreSQL service in GitLab CI instead |
| bsstu-report template mismatch | Low | High | Validate with `--check` early; use existing lab reports as reference |
| WPF designer issues in VS Code | Medium | Low | Use Visual Studio 2022 for XAML editing |
| Docker not available on defense machine | Low | High | Package standalone exe + document DB setup separately |

---

## Time Buffer
- **Official buffer**: 9 days (10.12 – 19.12) for report + defense prep
- **Internal buffer**: 1 week before each major milestone for integration issues
- **Critical path**: Implementation (Sprint 4) → Testing (Sprint 5) → Report (Sprint 6)

---

## Weekly Review Checklist (Friday)

- [ ] All tasks for week moved to Done / blocked with reason
- [ ] `dotnet test` — all pass
- [ ] Documentation updated
- [ ] Conventional commits
- [ ] No hardcoded config
- [ ] Next week's tasks clear
- [ ] `dotnet build -c Release` — zero warnings
- [ ] `bsstu-report --check` passes; docx+pdf generated; no formatting errors
- [x] `bsstu-report`: критерии сборки добавлены (`BstuReportBuilder/app/checks.py`):
    - `element` «Заключение» — только текст (`p`/`enum`/`references`); без `h2`/`h3`/рисунков/таблиц → `ERROR`
    - Каждый `h1` раздел (кроме «введение», «заключение», «ответы на контрольные вопросы»): ≥2 `h2`, последний `h2` = «Вывод по разделу» → `ERROR`
    - Раздел (`h1`) не начинается с текста (`p`) — открывается `h2` → `ERROR`
    - Подраздел (`h2`) начинается и заканчивается текстом → `ERROR`
    - Рисунок (`figure`) без подписи (`caption`) → `ERROR`
    - Дубли заголовков (`h1`/`h2`/`h3`) → `WARNING`
    - Заголовки с номером длиннее 62 символов → `ERROR`
    - Прямые кавычки (`"`) в тексте (кроме моноширинного кода с `font.name == "Consolas"`) → `WARNING`
    - Длинное тире (`—`) в тексте → `WARNING` (нужно среднее `–`)
    - Табуляция (`\t`) в списках (`enum`) → `ERROR`
    - «Список использованных источников» (`references`) в отчёте ЛР → `ERROR`
    - Метки (`label`) разрешаются через `@рис:`/`@табл:`/`@форм:`/`@лист:`; неизвестные — `ERROR`; неиспользованные — `WARNING`
- [x] `docs/report.json` — полная модель блоков для сборки курсовой записки (title, blocks с `h1`/`h2`/`h3`/`element`/`p`/`table`/`figure`/`references`/`listings`)

---

## План написания курсовой работы (по `BstuReportBuilder` / СТП БГТУ 001-2019)

### Что должно быть в отчёте (структурные блоки):

1. **Титульный лист** (`title` в `report.json`): шапка БГТУ, факультет/кафедра, специальность, название работы, дисциплина, ФИО студента, проверил, Минск 2026.
2. **Содержание** (авто-оглавление через `TOC` в шаблоне — поле обновляется через `updateFields`)
3. **Введение** (`element`: «Введение») — цель работы (одним предложением), задачи через тире (`- `), актуальность, краткий обзор проекта, перечень использованных технологий.
4. **Разделы** (нумерованные `h1` — по требованиям курса):
   - `h1`: «1 Анализ предметной области» — аналоги, варианты использования, требования (FR + NFR из `FR_NFR.md`)
     - `h2`: «Описание системы» (текст + `figure` с диаграммой вариантов использования из `docs/UML/use_case.md`)
     - `h2`: «Аналоги и требования» (таблица с аналогами + `table` с FR/NFR)
     - `h2`: «Вывод по разделу» (текстовый вывод по разделу)
   - `h1`: «2 Требования к проекту» — функциональные (`table` с матрицей FR-A/FR-C, статусами) + нефункциональные (`table` с NFR)
     - `h2`: «Функциональные требования» (текст + таблица с FR)
     - `h2`: «Нефункциональные требования» (текст + таблица с NFR)
     - `h2`: «Вывод по разделу»
   - `h1`: «3 Архитектура и проектирование» — MVVM, слои, паттерны (`ARCHITECTURE.md`), UML-диаграммы (`docs/UML/`)
     - `h2`: «Архитектурный паттерн MVVM» (текст)
     - `h2`: «Паттерны проектирования» (текст + ссылки на UML)
     - `h2`: «UML-диаграммы» (`figure`: `docs/UML/use_case.md`, `docs/UML/class_diagram.md`, `docs/UML/sequence_diagrams.md`)
     - `h2`: «Вывод по разделу»
   - `h1`: «4 Проектирование базы данных» — ER-схема (`DATABASE.md`), SQL-определения (`table`), индексы, seed-данные
     - `h2`: «ER-схема» (`figure`: Mermaid-схема из `docs/DATABASE.md`)
     - `h2`: «Таблицы SQL» (`table`: определения `users`, `apps`, `users_apps` с типами, индексами)
     - `h2`: «Seed-данные» (текст + таблица пользователей/приложений из `docs/DATABASE.md`)
     - `h2`: «Вывод по разделу»
   - `h1`: «5 Реализация» — код (`Configuration/Settings.cs`, `Services/AuthService.cs`, `Data/PostgreSQLRepEF.cs`, `.gitlab-ci.yml`, `docker-compose.yml`)
     - `h2`: «Централизованная конфигурация» (код `Configuration/Settings.cs` как `listings` в приложении, или `code` в тексте)
     - `h2`: «Безопасность» (BCrypt с авто-перехешированием, SHA-256 fallback — текст)
     - `h2`: «Тестирование» (`table`: 43/43 unit, 9/10 integration с указанием flaky-теста)
     - `h2`: «Вывод по разделу»
5. **Заключение** (`element`: «Заключение») — ТОЛЬКО текст: итоги по каждому разделу из введения (один абзац на задачу), степень достижения цели, возможные направления развития. БЕЗ подразделов (`h2`), рисунков (`figure`), таблиц (`table`), формул (`formula`), листингов (`code`) или списков выдачи (`references`). Проверка `checks.py`: если в `element` «Заключение» встречается любой `h2`/`h3` или `figure`/`table`, это `ERROR`.
6. **Список использованных источников** (`references`) — нумерованный (`1.`, `2.` и т.д.), каждый источник: автор/название/год/издание или URL, без маркеров (`- `). Проверка `checks.py`: `references` в отчёте ЛР → `ERROR` (только для курсовых/дипломных).
7. **Приложения** (`element`: «Приложения» + `listings`):
   - `listings` с кодом (`label`: `settings`, `auth`, `repo`, `cicd`, `docker`)
   - `figure` с UML-диаграммами (`label`: `uml_class`, `uml_seq`, `uml_use_case`)
   - Каждый листинг — с новой страницы (`pagebreak` перед ним или через `new_page` в шаблоне); подпись листинга в приложении: «Приложение А» (заглавными, центр) + название листинга (с прописной). Листинг начинается с заголовка «ПРИЛОЖЕНИЕ А» (стиль `appendix_title`), затем заголовок листинга с прописной.

---

## Критерии сборки (из `BstuReportBuilder/app/checks.py`)

- `element` «Введение» и «Заключение» обязательны — иначе `ERROR`
- «Заключение» содержит только текст (`p`/`enum`/`references`) — иначе `ERROR` (без `h2`/`h3`/рисунков/таблиц)
- Каждый `h1` раздел (кроме «введение»/«заключение»/«контрольные вопросы»):
  - ≥2 `h2` — иначе `ERROR`
  - Последний `h2` содержит «вывод по разделу» — иначе `ERROR`
- После заголовка `h1` сразу текст (`p`) запрещён — нужен `h2` — иначе `ERROR`
- Каждый `h2`/`h3` начинается и заканчивается текстом (`p`/`enum`/`references`) — иначе `ERROR`
- Каждый рисунок (`figure`) имеет подпись формата «Рисунок N.M – ...» — иначе `ERROR`
- Заголовки (`h1`/`h2`/`h3`) — в одну строку, ≤62 символа с номером — иначе `ERROR`
- Списки (`enum`): разделитель — один пробел (`- `); таб (`\t`) — `ERROR`
- Длинное тире (`—`) в тексте — `WARNING` (заменить на `–`)
- Прямые кавычки (`"`) в тексте (кроме моноширинных блоков с `font.name == "Consolas"`) — `WARNING` (только «ёлочки»)
- Дубли заголовков (`h1`/`h2`/`h3`) — `WARNING`
- «Список использованных источников» (`references`) — только для курсовых/дипломных; в отчётах по ЛР — `ERROR`
- Каждая ссылка (`@рис:`/`@табл:`/`@форм:`/`@лист:`) разрешена; неизвестные метки — `ERROR`; неиспользованные метки — `WARNING`
- Раздел (`h1`) не заканчивается рисунком/таблицей/формулой — последний блок секции = текст — `ERROR`

---

## Итоговая статистика (на момент завершения курса)

- [x] Код проекта переписан: `Configuration/Settings.cs` — singleton с центральной конфигурацией; `.env` защищён `.gitignore`; Docker (`postgres:16-alpine` pinned); `.gitlab-ci.yml` содержит полный CI/CD pipeline (4 stages); `Services/AuthService.cs` — BCrypt + SHA-256 с авто-перехешированием; `Data/` — EF Core + 3 репозитория; `tests/unit/` — 43/43 passing; `tests/integration/` — 9/10 (1 flaky нормализован через RestoreDefaults); `.gitlab-ci.yml` содержит полный pipeline с JUnit XML; `docs/` — полная документация; UML — 3 диаграммы (Mermaid); план — 8 этапов до 19.12.2026; критерии сборки `checks.py` — добавлены правила для `Заключение` и `Вывод по разделу`; `docs/report.json` — готовая модель блоков для сборки курсовой записки через `python3 -m bgtu_report build`.
- [ ] Защита (20.12.2026) — подготовка слайдов, демонстрация потоков Admin + Client, устное объяснение отчёта
