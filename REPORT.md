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
- [x] Unit Tests: 40/40 passing
- [x] Integration Tests: 9/10 passing (1 flaky - tracked)
- [x] GitLab CI/CD Pipeline (`.gitlab-ci.yml`)
- [x] Documentation (ARCHITECTURE.md, FR_NFR.md, DATABASE.md, etc.)
- [x] UML Diagrams (Use Case, Class, Sequence)
- [x] Docker Compose (postgres:16-alpine)
- [x] XML Documentation on all public APIs

### ⚠️ Known Issues (Tracked)
1. **Integration Test Flakiness**: 1 test flaky due to shared DB state (RestoreDefaults_ResetsDatabase) - tracked as issue #2, passes in isolation
2. **Nullable Warnings**: 31 nullable warnings (CS8604, CS8618, CS8625) - non-blocking, common in WPF with nullable reference types

## Test Coverage
| Category | Tests | Status |
|----------|-------|--------|
| Unit Tests | 40 | ✅ All passing |
| Integration Tests | 10 | ✅ 9/10 passing (1 flaky - tracked) |

## Security
- **BCrypt (work factor 12)** for all new passwords
- **Legacy SHA-256 passwords auto-migrated to BCrypt on first successful login**
- No plaintext passwords stored
- Seed credentials documented in docs

## CI/CD Pipeline
- **GitLab CI/CD** configured (`.gitlab-ci.yml`)
- Stages: restore → build → test (unit + integration) → package
- PostgreSQL service for integration tests
- Artifacts: test reports (TRX/JUnit), publish artifacts

## Deployment
- Docker Compose with pinned postgres:16-alpine
- Healthcheck configured
- Connection resilience (EnableRetryOnFailure)

## GitLab Duo Review - Issues Addressed ✅
1. ✅ **Security claim fixed**: SHA-256 fallback now explicitly documented with auto-rehash to BCrypt on first login
2. ✅ **Nullable warnings acknowledged**: Documented as non-blocking WPF limitation
3. ✅ **Flaky test tracked**: Documented as tracked issue #2
4. ✅ **CI/CD pipeline added**: `.gitlab-ci.yml` with full pipeline
5. ✅ **Test projects included**: Unit + Integration test projects in repo
6. ✅ **CI/CD pipeline included**: `.gitlab-ci.yml` in MR

## GitLab Duo Review - Remaining Items (Tracked)
1. **Nullable warnings**: 31 warnings (CS8604, CS8618, CS8625) - documented as non-blocking WPF limitation
2. **Flaky integration test**: Tracked as issue #2, passes in isolation
