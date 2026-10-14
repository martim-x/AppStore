# Codebase Metrics (pygount)

**Generated**: 2026-10-14  
**Tool**: pygount 3.2.0  
**Command**: `pygount --format=summary --folders-to-skip=".git,bin,obj,packages,.vs" .`

---

## Summary

| Language | Files | % | Code Lines | % | Comment Lines | % |
|----------|-------|---|------------|---|---------------|---|
| C# | 35 | 52.2% | 2,788 | 61.4% | 162 | 3.6% |
| Python | 1 | 1.5% | 116 | 62.0% | 14 | 7.5% |
| YAML | 1 | 1.5% | 24 | 92.3% | 0 | 0.0% |
| Markdown | 11 | 16.4% | 0 | 0.0% | 724 | 33.8% |
| __unknown__ | 17 | 25.4% | 0 | 0.0% | 0 | 0.0% |
| __binary__ | 2 | 3.0% | 0 | 0.0% | 0 | 0.0% |
| **Sum** | **67** | **100.0%** | **2,928** | **42.5%** | **900** | **13.1%** |

---

## Interpretation

- **Total source files**: 67 (excluding build artifacts)
- **Total code lines**: 2,928 (C# dominates at 2,788 lines)
- **Comment ratio**: 13.1% overall (C# only 3.6% — room for improvement)
- **Documentation**: 11 Markdown files (724 comment-equivalent lines)

---

## By Directory (C# only)

```
Data/           6 files  ~800 lines  (Models, Repository, EF, ADO, JSON)
ViewModels/     4 files  ~600 lines  (MainVM, AddEditVM, ProfileVM, Base)
Views/          14 files ~900 lines  (XAML + code-behind)
Services/       9 files  ~400 lines  (Auth, Theme, Locale, UndoRedo, Interfaces)
Commands/       2 files  ~100 lines  (RelayCommand, AppCommands)
Converters/     1 file   ~50 lines
Configuration/  1 file   ~80 lines  (Settings.cs)
ApplicationApp  2 files  ~100 lines  (Bootstrap)
```

---

## Target for Coursework

| Metric | Current | Target | Action |
|--------|---------|--------|--------|
| C# Code Lines | 2,788 | ~3,500+ | Implement missing features (Admin CRUD, full Client flows) |
| Comment Ratio (C#) | 3.6% | ≥15% | Add XML docs on public APIs |
| Test Coverage | 0% | ≥80% | Add unit + integration tests |
| Documentation Files | 11 | 15+ | Complete UML, CONFIGURATION, etc. |

---

## Next Measurement

Re-run after each sprint to track growth:

```bash
pygount --format=summary --folders-to-skip=".git,bin,obj,packages,.vs" . > docs/CODEBASE_METRICS_$(date +%F).md
```