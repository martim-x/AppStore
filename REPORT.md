# AppStore WPF - Project Report

## Project Overview
- **Project**: AppStore WPF Desktop Application
- **Framework**: .NET 8, WPF, MVVM
- **Database**: PostgreSQL 16 (Docker)
- **Coursework**: БГТУ, ИСиТ 3к 2гр, Марушко Т.Ф.

## Architecture Summary

### Layers
1. **Presentation** (Views) - XAML + code-behind
2. **ViewModels** - MVVM with INotifyPropertyChanged
3. **Services** - Business logic (Auth, Theme, Localization, UndoRedo)
4. **Data/Repository** - IRepository with 3 implementations (EF Core, ADO.NET, JSON)

### Design Patterns Used
1. **MVVM** - Primary architecture
2. **Repository** - 3 interchangeable implementations
3. **Command** - RelayCommand + Undo/Redo actions
4. **Strategy** - Repository implementations
5. **Factory** - View/ViewModel creation
6. **Observer** - INotifyPropertyChanged

## Implementation Status

### ✅ Completed
- [x] Centralized Configuration (Settings.cs)
- [x] DI Container (Microsoft.Extensions.DI)
- [x] EF Core Migrations + EnableRetryOnFailure
- [x] BCrypt Password Hashing + SHA-256 fallback **with auto-rehash on login**
- [x] Unit Tests: 43/43 passing
- [x] Integration Tests: 10/10 passing
- [x] GitLab CI/CD Pipeline (`.gitlab-ci.yml`) - Fixed platform issues
- [x] Documentation (ARCHITECTURE.md, FR_NFR.md, DATABASE.md, etc.)
- [x] UML Diagrams (Use Case, Class, Sequence)
- [x] Docker Compose (postgres:16-alpine)
- [x] XML Documentation on all public APIs

## ⚠️ Known Issues (Resolved)

1. **Integration Test Flakiness**: Resolved - `RestoreDefaults_ResetsDatabase` uses deterministic baseline after `RestoreDefaults()` call, passes reliably in CI
2. **Nullable Warnings**: Resolved - `CS8625` in `TestUndoRedoAction` fixed with nullable `Action?` parameters; WPF view patterns addressed with explicit constructor validation

## Test Coverage
|| Category | Tests | Status ||
||----------|-------|--------|
|| Unit Tests | 43 | ✅ All passing ||
|| Integration Tests | 10 | ✅ 10/10 passing ||

## Security
- **BCrypt (work factor 12)** for all new passwords
- **Legacy SHA-256 passwords auto-migrated to BCrypt on first successful login**
- No plaintext passwords stored
- Seed credentials documented in docs

## CI/CD Pipeline
- **GitLab CI/CD** configured (`.gitlab-ci.yml`) - Platform issues fixed
- Stages: restore → build → test (unit + integration) → package
- Linux runners: restore/build/test with EnableWindowsTargeting=true
- Windows runner: package with Windows Server Core image (Docker) or shell runner
- PostgreSQL service for integration tests on Linux runner
- Artifacts: test reports (JUnit XML), publish artifacts

## Deployment
- Docker Compose with pinned postgres:16-alpine
- Healthcheck configured
- Connection resilience (EnableRetryOnFailure)

## GitLab Duo Review - Issues Addressed ✅
1. ✅ **Security claim fixed**: SHA-256 fallback now explicitly documented with auto-rehash to BCrypt on first login
2. ✅ **Nullable warnings resolved**: CS8625 in tests fixed; WPF view patterns updated with explicit validation
3. ✅ **Flaky test resolved**: RestoreDefaults_ResetsDatabase uses deterministic baseline
4. ✅ **CI/CD pipeline added**: `.gitlab-ci.yml` with full pipeline
5. ✅ **Test projects included**: Unit + Integration test projects in repo
6. ✅ **CI/CD pipeline included**: `.gitlab-ci.yml` in MR
7. ✅ **Platform fixes**: restore job EnableWindowsTargeting, package job Windows Server Core image
8. ✅ **ValueComparer indentation fixed**: Aligned to class-member level in PostgreSQLRepEF.cs
9. ✅ **ValueComparer null safety**: All three lambdas now consistent with null checks
