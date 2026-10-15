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
- [x] BCrypt Password Hashing + SHA-256 fallback
- [x] Unit Tests: 40/40 passing
- [x] Integration Tests: 9/10 passing
- [x] Documentation (ARCHITECTURE.md, FR_NFR.md, DATABASE.md, etc.)
- [x] UML Diagrams (Use Case, Class, Sequence)
- [x] Docker Compose (postgres:16-alpine)
- [x] GitLab CI/CD ready

### ⚠️ Known Issues
1. **Integration Test Flakiness**: 1 test flaky due to shared DB state (RestoreDefaults_ResetsDatabase) - passes in isolation
2. **Nullable Warnings**: 31 nullable warnings (CS8604, CS8618, CS8625) - non-blocking

## Test Coverage
| Category | Tests | Status |
|----------|-------|--------|
| Unit Tests | 40 | ✅ All passing |
| Integration Tests | 10 | ✅ 9/10 passing |

## Security
- BCrypt (work factor 12) for new passwords
- SHA-256 fallback for legacy seed data
- No plaintext passwords stored

## Deployment
- Docker Compose with pinned postgres:16-alpine
- Healthcheck configured
- Connection resilience (EnableRetryOnFailure)

## GitLab Duo Review Request
Please review:
1. Architecture correctness
2. Security practices
3. Test coverage adequacy
4. Code quality and maintainability
5. WPF/MVVM best practices adherence