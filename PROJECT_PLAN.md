# AppStore Coursework — Execution Plan (Epic → Story → Task)

**Deadline**: 19.12.2026  
**Current date**: 14.10.2026  
**Weeks remaining**: ~9

---

## Epic 1: Foundation & Documentation (Weeks 1–2) — *20.09 – 27.09 per assignment*

### Story 1.1: Repository Setup & Clean Baseline
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Copy source to new repo, clean git history | T-1.1.1 | ✅ Done | Already completed during survey |
| Add proper `.gitignore` (exclude bin/obj, .env, IDE) | T-1.1.2 | ✅ Done | |
| Create `.env.example` template | T-1.1.3 | ✅ Done | |
| Pin Docker image to `postgres:16-alpine` (no latest) | T-1.1.4 | ✅ Done | Healthcheck, restart policy added |
| Verify `dotnet build` / `dotnet run` works locally | T-1.1.5 | ⬜ Todo | Need to test after Settings integration |

### Story 1.2: Centralized Configuration
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Create `Configuration/Settings.cs` (singleton, .env + env vars) | T-1.2.1 | ✅ Done | Created, not yet integrated |
| Replace all hardcoded DB strings with `Settings.Instance.ConnectionString` | T-1.2.2 | ⬜ Todo | PostgreSQLRepEF, PostgreSQLRepADO, ApplicationApp |
| Replace hardcoded SHA-256, defaults (theme, lang, page size) | T-1.2.3 | ⬜ Todo | AuthService, ThemeService, MainViewModel |
| Add validation on startup (throw if required config missing) | T-1.2.4 | ⬜ Todo | |
| Document config in `docs/CONFIGURATION.md` | T-1.2.5 | ⬜ Todo | |

### Story 1.3: Project Documentation Suite
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Create `README.md` (overview, stack, run instructions, structure) | T-1.3.1 | ✅ Done | |
| Create `docs/ARCHITECTURE.md` (layers, Mermaid diagrams, patterns) | T-1.3.2 | ✅ Done | |
| Create `docs/FR_NFR.md` (functional & non-functional reqs from assignment) | T-1.3.3 | ⬜ Todo | |
| Create `docs/DATABASE.md` (ER diagram, schema, seed data) | T-1.3.4 | ⬜ Todo | |
| Create `docs/UML/` with Use Case, Class, Sequence (Mermaid) | T-1.3.5 | ⬜ Todo | |
| Create `docs/PROJECT_PLAN.md` (calendar plan from assignment) | T-1.3.6 | ⬜ Todo | |
| Create `BACKLOG.md` (this file) | T-1.3.7 | ✅ Done | |

### Story 1.4: Codebase Indexing & Metrics
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Run `codebase-memory` index on new repo | T-1.4.1 | ⬜ Todo | `index_repository` with explicit name |
| Run `codebase-inspection` (pygount) for LOC baseline | T-1.4.2 | ⬜ Todo | Save output to `docs/CODEBASE_METRICS.md` |
| Verify graph search works for key symbols | T-1.4.3 | ⬜ Todo | |

---

## Epic 2: Architecture Hardening & Design Patterns (Weeks 2–3) — *27.09 – 06.10 per assignment*

### Story 2.1: Dependency Injection & Service Locator Cleanup
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Add `Microsoft.Extensions.DependencyInjection` NuGet | T-2.1.1 | ⬜ Todo | |
| Create `CompositionRoot.cs` — register all services/repos | T-2.1.2 | ⬜ Todo | |
| Refactor `ApplicationApp` to use `IServiceProvider` | T-2.1.3 | ⬜ Todo | |
| Remove manual `new ThemeService()` etc. from ViewModels | T-2.1.4 | ⬜ Todo | |
| Document DI setup in `docs/ARCHITECTURE.md` | T-2.1.5 | ⬜ Todo | |

### Story 2.2: Repository Layer — EF Core Migrations
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Add EF Core Migrations (replace `EnsureDatabaseCreated`) | T-2.2.1 | ⬜ Todo | `dotnet ef migrations add InitialCreate` |
| Create `IDbInitializer` for seed data (separate from migrations) | T-2.2.2 | ⬜ Todo | |
| Add connection resilience (EnableRetryOnFailure) | T-2.2.3 | ⬜ Todo | |
| Document migration strategy in `docs/DATABASE.md` | T-2.2.4 | ⬜ Todo | |

### Story 2.3: Security — Password Hashing Upgrade Path
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Add `BCrypt.Net-Next` NuGet (or `Argon2`) | T-2.3.1 | ⬜ Todo | |
| Create `IPasswordHasher` interface + `BCryptHasher` impl | T-2.3.2 | ⬜ Todo | |
| Update `AuthService` to use `IPasswordHasher` | T-2.3.3 | ⬜ Todo | |
| Add migration script to re-hash existing passwords | T-2.3.4 | ⬜ Todo | |
| Keep SHA-256 as fallback for compat (configurable) | T-2.3.5 | ⬜ Todo | |

---

## Epic 3: Core Features — Admin (Weeks 3–5) — *06.10 – 20.10 per assignment*

### Story 3.1: App CRUD (Admin)
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Add/Edit dialog — validation (required fields, price ≥ 0, etc.) | T-3.1.1 | ⬜ Todo | `AddEditAppViewModel` + `AddEditWindow` |
| Delete with confirmation dialog | T-3.1.2 | ⬜ Todo | `ConfirmDialog` exists, wire up |
| Restore defaults with confirmation | T-3.1.3 | ⬜ Todo | `RestoreDefaultsCommand` exists, wire up |
| Admin-only UI visibility (IsAdmin binding) | T-3.1.4 | ⬜ Todo | `MainViewModel.IsAdmin` already exposed |

### Story 3.2: Catalog Management (Admin + Client shared)
| Task | ID | Status | Notes |
|------|----|--------|-------|
| "Pick of the Day" — featured apps section | T-3.2.1 | ⬜ Todo | `MainViewModel.FeaturedApps` + `FeaturedBannerView` |
| Categories sidebar navigation | T-3.2.2 | ⬜ Todo | `NavCategoryMap` exists, verify UI |
| Full app list with pagination/virtualization | T-3.2.3 | ⬜ Todo | `_visibleCount` + "See All" toggle exists |

---

## Epic 4: Core Features — Client (Weeks 5–7) — *20.10 – 14.11 per assignment*

### Story 4.1: Search, Filter, Sort
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Search by name, developer, category, tags | T-4.1.1 | ⬜ Todo | `MainViewModel.SearchQuery` logic exists, verify |
| Filter: price range, min rating, tags, installed only | T-4.1.2 | ⬜ Todo | `MinPrice`, `MaxPrice`, `MinRating`, `DownloadedOnly` exist |
| Sort: name, price, rating, release date | T-4.1.3 | ⬜ Todo | `SortBy` property exists, verify all options |
| Tag cloud / tag filter chips | T-4.1.4 | ⬜ Todo | `AllTags` + `SelectTagCommand` exist |

### Story 4.2: App Detail & Install/Uninstall with Undo/Redo
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Detail view — all metadata fields | T-4.2.1 | ⬜ Todo | `AppDetailView` exists, verify binding |
| Install/Uninstall command with Undo/Redo | T-4.2.2 | ⬜ Todo | `DownloadCommand` + `UndoRedoService` implemented, verify |
| Undo/Redo toolbar buttons (enable/disable per CanExecute) | T-4.2.3 | ⬜ Todo | `UndoCommand`/`RedoCommand` exist |

### Story 4.3: Profile & Settings (Client)
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Edit first/last name, email | T-4.3.1 | ⬜ Todo | `ProfileViewModel` + `ProfilePopup` exist |
| Change password (old + new confirmation) | T-4.3.2 | ⬜ Todo | `AuthService.ChangePassword` exists |
| Language switch (RU/EN) — persists | T-4.3.3 | ⬜ Todo | `LocalizationService` + `SwitchLanguageCommand` exist |
| Theme switch (Light/Dark) — persists | T-4.3.4 | ⬜ Todo | `ThemeService` exists |
| Logout → return to Login | T-4.3.5 | ⬜ Todo | `LogoutCommand` + `LogoutRequested` event exist |

---

## Epic 5: Testing (Weeks 7–8) — *14.11 – 28.11 per assignment*

### Story 5.1: Test Infrastructure
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Create `tests/unit/AppStore.UnitTests` (xUnit, Moq) | T-5.1.1 | ⬜ Todo | Reference main project |
| Create `tests/integration/AppStore.IntegrationTests` (xUnit, Testcontainers) | T-5.1.2 | ⬜ Todo | PostgreSQL container |
| Add `coverlet.collector` + `ReportGenerator` for coverage | T-5.1.3 | ⬜ Todo | |
| Configure CI pipeline (GitLab CI) to run tests on push | T-5.1.4 | ⬜ Todo | |

### Story 5.2: Unit Tests (Target: ≥80% business logic)
| Area | Target Classes | Task ID | Status |
|------|----------------|---------|--------|
| Repository (JSON, EF, ADO) | `JsonRep`, `PostgreSQLRepEF`, `PostgreSQLRepADO` | T-5.2.1 | ⬜ Todo |
| Services | `AuthService`, `ThemeService`, `LocalizationService`, `UndoRedoService` | T-5.2.2 | ⬜ Todo |
| ViewModels | `MainViewModel`, `AddEditAppViewModel`, `ProfileViewModel` | T-5.2.3 | ⬜ Todo |
| Commands | `RelayCommand`, `GetAction`, `OpenAction` | T-5.2.4 | ⬜ Todo |
| Converters | All in `Converters.cs` | T-5.2.5 | ⬜ Todo |
| Models | `App` (FinalPrice, ButtonLabel), `User` (AvatarLetter) | T-5.2.6 | ⬜ Todo |

### Story 5.3: Integration Tests
| Scenario | Task ID | Status |
|----------|---------|--------|
| DB migration + seed + query | T-5.3.1 | ⬜ Todo |
| Full auth flow (login → session → logout) | T-5.3.2 | ⬜ Todo |
| App CRUD via repository | T-5.3.3 | ⬜ Todo |
| Install/Uninstall + Undo/Redo persistence | T-5.3.4 | ⬜ Todo |
| Multi-repository parity (EF vs ADO vs JSON) | T-5.3.5 | ⬜ Todo |

---

## Epic 6: Coursework Report & Defense Prep (Weeks 8–9) — *28.11 – 10.12 per assignment*

### Story 6.1: Report Generation (bsstu-report)
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Create `report.json` with all blocks (h1/h2/h3/p/enum/figure/table/code) | T-6.1.1 | ⬜ Todo | Follow bsstu-report format |
| Include UML diagrams as figures (render Mermaid → PNG) | T-6.1.2 | ⬜ Todo | `mmdc` or online renderer |
| Include code listings in `listings` section (appendix) | T-6.1.3 | ⬜ Todo | |
| Build docx + pdf: `python3 -m app build report.json -o Курсовая.docx --pdf --check` | T-6.1.4 | ⬜ Todo | Run in BstuReportBuilder |
| Verify `--check` passes (no warnings/errors) | T-6.1.5 | ⬜ Todo | |

### Story 6.2: User Manual & Appendix
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Write "Руководство пользователя" (install, run, features per role) | T-6.2.1 | ⬜ Todo | |
| Prepare code listings for appendix (key files only) | T-6.2.2 | ⬜ Todo | |
| Create presentation slides (10–15 min defense) | T-6.2.3 | ⬜ Todo | |

---

## Epic 7: Final Delivery & GitLab (Week 9) — *10.12 – 19.12 per assignment*

### Story 7.1: GitLab Push & CI/CD
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Create GitLab repo, push `main` | T-7.1.1 | ⬜ Todo | |
| Configure GitLab CI (build, test, coverage) | T-7.1.2 | ⬜ Todo | `.gitlab-ci.yml` |
| Enable GitLab Duo code review | T-7.1.3 | ⬜ Todo | |
| Address Duo findings (security, style, perf) | T-7.1.4 | ⬜ Todo | |

### Story 7.2: Final Verification
| Task | ID | Status | Notes |
|------|----|--------|-------|
| Clean build: `dotnet build -c Release` | T-7.2.1 | ⬜ Todo | |
| Full test suite passes | T-7.2.2 | ⬜ Todo | |
| Manual smoke test: Login → Admin CRUD → Client flow → Logout | T-7.2.3 | ⬜ Todo | |
| Package installation artifacts (publish single-file?) | T-7.2.4 | ⬜ Todo | `dotnet publish -r win-x64 -p:PublishSingleFile=true` |
| Submit to instructor (docx + pdf + source + install) | T-7.2.5 | ⬜ Todo | |

---

## Cross-Cutting Concerns (Continuous)

| Concern | Tasks |
|---------|-------|
| **Code Quality** | Run `dotnet format` / EditorConfig on every commit; no warnings in build |
| **Documentation Sync** | Update `docs/` when architecture changes |
| **Config Hygiene** | No hardcoded values — all via `Settings.Instance` |
| **Git Hygiene** | Conventional commits; feature branches for stories; PR for each story |

---

## Definition of Ready (Story)
- [ ] Acceptance criteria clear
- [ ] Dependencies identified
- [ ] Test approach defined (unit + integration)
- [ ] Config changes documented

## Definition of Done (Task)
- [ ] Code compiles (Release, no warnings)
- [ ] Unit tests added + pass
- [ ] Integration tests added + pass (if applicable)
- [ ] Documentation updated
- [ ] No hardcoded config
- [ ] Self-reviewed + GitLab Duo review passed

---

## Immediate Next Actions (This Session)
1. **T-1.1.5** — Test build/run with current state
2. **T-1.2.2–T-1.2.4** — Integrate `Settings.cs` into all consumers
3. **T-1.3.3–T-1.3.6** — Create missing docs (FR_NFR, DATABASE, UML, PROJECT_PLAN)
4. **T-1.4.1–T-1.4.3** — Index codebase, run pygount

---

**Approval Required**: Confirm this plan structure before I start executing tasks. Each story will be a separate work session with verification at Done.