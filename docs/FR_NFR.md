# Functional & Non-Functional Requirements

**Source**: University/лист задания Марушко Т.Ф. ИСиТ 3к 2 гр.docx (Section 2.1–2.2)  
**Discipline**: Объектно-ориентированные технологии программирования и стандарты проектирования

---

## Functional Requirements (FR)

### FR-A: Administrator Functions
| ID | Requirement | Priority | Status | Notes |
|----|-------------|----------|--------|-------|
| FR-A01 | Login with admin credentials | Must | ⬜ Todo | AuthService.Login, role check |
| FR-A02 | View app catalog with search, filter, sort | Must | ⬜ Partial | MainViewModel has logic, verify UI |
| FR-A03 | Install/Uninstall apps | Must | ⬜ Partial | DownloadCommand exists |
| FR-A04 | Manage profile (name, email, password) | Must | ⬜ Partial | ProfileViewModel exists |
| FR-A05 | **Add new app** to catalog (all fields) | Must | ⬜ Todo | AddEditWindow + ViewModel |
| FR-A06 | **Edit any app** in catalog | Must | ⬜ Todo | Same dialog, pre-filled |
| FR-A07 | **Delete any app** with confirmation | Must | ⬜ Todo | DeleteCommand + ConfirmDialog |
| FR-A08 | **Restore default catalog** with confirmation | Must | ⬜ Todo | RestoreDefaultsCommand + ConfirmDialog |
| FR-A09 | Admin functions visible **only for Admin role** | Must | ⬜ Todo | `IsAdmin` binding in XAML |

#### App Fields (FR-A05/FR-A06)
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| ShortName | string | Yes | Max 50 chars |
| FullName | string | Yes | Max 100 chars |
| Description | string | Yes | Max 2000 chars |
| Developer | string | Yes | Max 100 chars |
| Category | string | Yes | From predefined list |
| Rating | double | Yes | 0.0–5.0 |
| RatingCount | int | Yes | ≥0 |
| Price | double | Yes | ≥0 |
| Version | string | Yes | SemVer format |
| SizeMB | double | Yes | >0 |
| Country | string | Yes | ISO 3166-1 alpha-2 |
| AgeRating | string | Yes | "4+", "9+", "12+", "17+" |
| Color | string | Yes | Hex #RRGGBB |
| IsFeatured | bool | No | Default false |
| DiscountPercent | double? | No | 0–100 or null |
| ReleaseDate | DateTime | Yes | Not future |
| Tags | List<string> | No | Comma-separated input |

### FR-C: Client Functions
| ID | Requirement | Priority | Status | Notes |
|----|-------------|----------|--------|-------|
| FR-C01 | Login with user credentials | Must | ⬜ Todo | Same AuthService |
| FR-C02 | View catalog: "Pick of the Day", Categories, Full List | Must | ⬜ Partial | FeaturedApps + NavCategoryMap |
| FR-C03 | View app detail (all metadata) | Must | ⬜ Partial | AppDetailView exists |
| FR-C04 | Search by name, developer, category, tags | Must | ⬜ Partial | SearchQuery logic exists |
| FR-C05 | Filter by price range, min rating, tags, installed only | Must | ⬜ Partial | MinPrice, MaxPrice, MinRating, DownloadedOnly |
| FR-C06 | Sort by name, price, rating, release date | Must | ⬜ Partial | SortBy property exists |
| FR-C07 | Install/Uninstall apps | Must | ⬜ Partial | DownloadCommand |
| FR-C08 | **Undo/Redo** last install/uninstall | Must | ⬜ Partial | UndoRedoService + Commands |
| FR-C09 | Profile: view/edit first name, last name, email | Must | ⬜ Partial | ProfileViewModel |
| FR-C10 | Change password (old + new + confirm) | Must | ⬜ Partial | AuthService.ChangePassword |
| FR-C11 | Switch language (RU/EN) | Must | ⬜ Partial | LocalizationService |
| FR-C12 | Switch theme (Light/Dark) | Must | ⬜ Partial | ThemeService |
| FR-C13 | Logout → return to login | Must | ⬜ Partial | LogoutCommand + event |

---

## Non-Functional Requirements (NFR)

| ID | Category | Requirement | Metric / Acceptance | Priority |
|----|----------|-------------|---------------------|----------|
| NFR-01 | Architecture | MVVM pattern strictly followed | No code-behind logic; VM knows nothing of View | Must |
| NFR-02 | Architecture | Business logic separated from UI | Services/Repository testable without WPF | Must |
| NFR-03 | Design Patterns | ≥2 patterns used and documented | MVVM + Repository + Command + Strategy (4 total) | Must |
| NFR-04 | Design Patterns | UML diagrams: Use Case, Class, Sequence | Mermaid in docs/UML/ | Must |
| NFR-05 | Language | C# (.NET 8), Windows desktop (WPF) | TargetFramework net8.0-windows | Must |
| NFR-06 | Database | PostgreSQL in Docker | docker-compose.yml, pinned version | Must |
| NFR-07 | Database | EF Core 8 + Npgsql (primary) | Microsoft.EntityFrameworkCore 8.x | Must |
| NFR-08 | Database | Alternative: ADO.NET, JSON | PostgreSQLRepADO, JsonRep exist | Should |
| NFR-09 | Security | Password hashing (SHA-256 min, BCrypt target) | No plaintext passwords | Must |
| NFR-10 | Security | Role-based UI (Admin vs User) | IsAdmin binding, server-side check | Must |
| NFR-11 | Usability | Intuitive, responsive UI | No frozen UI >200ms; async where needed | Must |
| NFR-12 | Localization | RU/EN runtime switch | ResourceDictionary swap | Must |
| NFR-13 | Theming | Light/Dark runtime switch | ResourceDictionary swap | Must |
| NFR-14 | Config | No hardcoded values | All via Settings.cs / .env | Must |
| NFR-15 | Config | No `latest` Docker tags | Pinned `postgres:16-alpine` | Must |
| NFR-16 | Testing | Unit tests ≥80% business logic | xUnit, Moq, coverlet | Must |
| NFR-17 | Testing | Integration tests (DB, auth, CRUD) | PostgreSQL 16 service container | Must |
| NFR-18 | Documentation | Coursework report per СТП БГТУ 001-2019 | bsstu-report builder, docx+pdf | Must |
| NFR-19 | Documentation | User manual included | Appendix in report | Must |
| NFR-20 | Code Quality | Zero compiler warnings | `dotnet build -c Release` | Must |
| NFR-21 | Code Quality | Consistent formatting | EditorConfig + `dotnet format` | Should |
| NFR-22 | Performance | App list virtualization / pagination | 6 items default, "See All" toggle | Should |
| NFR-23 | Reliability | Connection retry (EF Core) | `EnableRetryOnFailure` | Should |
| NFR-24 | Maintainability | DI container (Microsoft.Extensions.DI) | CompositionRoot, no manual `new` | Should |

---

## Traceability Matrix (FR → Implementation)

| FR | ViewModel / Service | View | Repository Method |
|----|---------------------|------|-------------------|
| FR-A01, FR-C01 | AuthService.Login | LoginWindow | GetUserByLogin |
| FR-A02, FR-C02 | MainViewModel (Apps, FeaturedApps) | MainWindow | GetAllApps |
| FR-A03, FR-C07 | MainViewModel.DownloadCommand | AppRowView / AppDetailView | DownloadApp / UninstallApp |
| FR-A04, FR-C09 | ProfileViewModel | ProfilePopup | UpdateUser |
| FR-C10 | AuthService.ChangePassword | ProfilePopup | UpdateUser |
| FR-A05, FR-A06 | AddEditAppViewModel | AddEditWindow | AddApp / UpdateApp |
| FR-A07 | MainViewModel.DeleteCommand | MainWindow (context menu) | DeleteApp |
| FR-A08 | MainViewModel.RestoreDefaultsCommand | MainWindow (menu) | RestoreDefaults |
| FR-C04 | MainViewModel.SearchQuery | MainWindow (search box) | GetAllApps (filtered in VM) |
| FR-C05 | MainViewModel filters | MainWindow (filter panel) | GetAllApps (filtered in VM) |
| FR-C06 | MainViewModel.SortBy | MainWindow (sort combo) | GetAllApps (sorted in VM) |
| FR-C08 | UndoRedoService + Commands | MainWindow (toolbar) | DownloadApp / UninstallApp |
| FR-C11 | LocalizationService + SwitchLanguageCommand | MainWindow (menu) | — |
| FR-C12 | ThemeService | App.xaml (ResourceDictionary) | — |
| FR-C13 | MainViewModel.LogoutCommand | MainWindow (profile menu) | — |
| FR-A09 | MainViewModel.IsAdmin | All admin UI (Visibility binding) | — |

---

## Acceptance Criteria (per Assignment)

1. **Admin** can add/edit/delete apps, restore defaults, all with confirmations
2. **Client** sees Pick of the Day, categories, full list; can search/filter/sort
3. **Client** sees full app detail (description, dev, version, size, date, country, age, discount, tags)
4. **Client** install/uninstall works with **Undo/Redo** (last action)
5. **Profile**: edit name, email, change password
6. **Language** (RU/EN) and **Theme** (Light/Dark) switch at runtime, persist
7. **Logout** returns to login screen
8. **Admin UI hidden** for non-admin users
9. **MVVM** — no business logic in code-behind
10. **UML** — Use Case, Class, Sequence diagrams
11. **2+ patterns** — documented (we have 4+)
12. **Report** — docx + pdf via bsstu-report, СТП БГТУ 001-2019
13. **Deadline** — 19.12.2026