# Calendar Plan — Coursework (per Assignment)

**Assignment**: University/лист задания Марушко Т.Ф. ИСиТ 3к 2 гр.docx (Section "Календарный план")  
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
- [ ] `dotnet build -c Release` — zero warnings
- [ ] `dotnet test` — all pass
- [ ] Documentation updated for any architectural changes
- [ ] Git commits follow conventional format
- [ ] No hardcoded config introduced
- [ ] Next week's tasks clear and assigned