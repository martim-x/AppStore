using Project.Commands;
using AppStore.Core.Data;
using Project.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Project.ViewModels
{
    /// <summary>
    /// Root ViewModel. Owns all state visible to the main window:
    /// the current app list, filter settings, navigation state, and commands.
    /// Uses IRepository directly — no AppService layer.
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly IRepository _repo;
        private readonly ILocalizationService _localization;
        private readonly IAuthService _auth;
        private readonly IThemeService _theme;
        private readonly IUndoRedoService _undoRedo = new UndoRedoService();

        // Public so Views can access for add/edit dialogs
        public IRepository Repository => _repo;

        // Maps sidebar nav key → Category string used in Filter (null = no category filter)
        private static readonly Dictionary<string, string?> NavCategoryMap = new()
        {
            ["Discover"] = null,
            ["Create"] = "Design",
            ["Work"] = "Productivity",
            ["Play"] = "Games",
            ["Develop"] = "Development",
            ["Installed"] = null,
            ["Categories"] = null,
        };

        // ── backing fields ──────────────────────────────────────────────────
        private ObservableCollection<App> _apps = new();
        private ObservableCollection<App> _featuredApps = new();
        private ObservableCollection<string> _allTags = new();
        private App? _selectedApp;
        private string _searchQuery = string.Empty;
        private string _selectedCategory = "Discover";
        private string? _selectedTag;
        private double _minPrice;
        private double _maxPrice = 1000;
        private double _minRating;
        private string _sortBy = "featured";
        private bool _downloadedOnly;
        private bool _isDetailOpen;
        private bool _isCategoryGridOpen;
        private string _currentLanguage = "en";
        private int _visibleCount = 6;
        private int? _userRating;

        // ── observable collections ──────────────────────────────────────────
        public ObservableCollection<App> Apps
        {
            get => _apps;
            set => SetField(ref _apps, value);
        }
        public ObservableCollection<App> FeaturedApps
        {
            get => _featuredApps;
            set => SetField(ref _featuredApps, value);
        }
        public ObservableCollection<string> AllTags
        {
            get => _allTags;
            set => SetField(ref _allTags, value);
        }

        // ── selected app ────────────────────────────────────────────────────
        public App? SelectedApp
        {
            get => _selectedApp;
            set
            {
                SetField(ref _selectedApp, value);
                if (value != null)
                    IsDetailOpen = true;

                LoadUserRating();
            }
        }

        /// <summary>
        /// Оценка текущего пользователя для выбранного приложения (APP-003).
        /// null — пользователь ещё не оценивал приложение.
        /// </summary>
        public int? UserRating
        {
            get => _userRating;
            set
            {
                if (!SetField(ref _userRating, value))
                    return;

                OnPropertyChanged(nameof(UserRatingText));
                OnPropertyChanged(nameof(HasUserRating));
            }
        }

        /// <summary>Текст «Ваша оценка: N» для панели деталей.</summary>
        public string UserRatingText => _userRating.HasValue ? _userRating.Value.ToString() : "—";

        /// <summary>Поставлена ли пользователем оценка выбранному приложению.</summary>
        public bool HasUserRating => _userRating.HasValue;

        // ── filter / search state ───────────────────────────────────────────
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (!SetField(ref _searchQuery, value))
                    return;

                OnPropertyChanged(nameof(HasSearchText));

                Debug.WriteLine($"[DataTrigger test] SearchQuery = '{_searchQuery}'");
                Debug.WriteLine($"[DataTrigger test] HasSearchText = {HasSearchText}");

                _visibleCount = 6;
                _selectedTag = null;
                RefreshList();
            }
        }

        public bool HasSearchText => !string.IsNullOrWhiteSpace(_searchQuery);


        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (!SetField(ref _selectedCategory, value))
                    return;
                _selectedTag = null;
                _visibleCount = 6;
                _downloadedOnly = value == "Installed";
                RefreshList();
                OnPropertyChanged(nameof(SelectedCategoryLabel));
                OnPropertyChanged(nameof(IsFeaturedVisible));
            }
        }

        public double MinPrice
        {
            get => _minPrice;
            set
            {
                if (SetField(ref _minPrice, value))
                    RefreshList();
            }
        }
        public double MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (SetField(ref _maxPrice, value))
                    RefreshList();
            }
        }
        public double MinRating
        {
            get => _minRating;
            set
            {
                if (SetField(ref _minRating, value))
                    RefreshList();
            }
        }
        public string SortBy
        {
            get => _sortBy;
            set
            {
                if (SetField(ref _sortBy, value))
                    RefreshList();
            }
        }
        public bool DownloadedOnly
        {
            get => _downloadedOnly;
            set
            {
                if (SetField(ref _downloadedOnly, value))
                    RefreshList();
            }
        }

        // ── navigation state ────────────────────────────────────────────────
        public bool IsDetailOpen
        {
            get => _isDetailOpen;
            set => SetField(ref _isDetailOpen, value);
        }
        public bool IsCategoryGridOpen
        {
            get => _isCategoryGridOpen;
            set => SetField(ref _isCategoryGridOpen, value);
        }
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set => SetField(ref _currentLanguage, value);
        }

        // ── computed ────────────────────────────────────────────────────────
        /// <summary>
        /// Заголовок страницы: тег фильтра либо локализованное название
        /// категории (навигация и категории каталога — English-строки в БД,
        /// отображение локализуется через словари Cat…/Cat01…Cat10).
        /// </summary>
        public string SelectedCategoryLabel
        {
            get
            {
                if (_selectedTag != null)
                    return _selectedTag;

                var label = LocalizeCategory(_selectedCategory);
                return label ?? _selectedCategory;
            }
        }

        /// <summary>Локализовать категорию/раздел, если есть ключ локализации.</summary>
        private static string? LocalizeCategory(string? category)
        {
            if (string.IsNullOrEmpty(category))
                return category;

            // Навигационные разделы: CatDiscover, CatCreate, CatWork, CatPlay,
            // CatDevelop, CatCategories, CatInstalled.
            var navKey = "Cat" + category;
            if (Application.Current.TryFindResource(navKey) is string navLabel)
                return navLabel;

            // Категории каталога (данные): Cat01Development…Cat10Health.
            var dataKey = category switch
            {
                "Development" => "Cat01Development",
                "Design" => "Cat02Design",
                "Productivity" => "Cat03Productivity",
                "Games" => "Cat04Games",
                "Media" => "Cat05Media",
                "Social" => "Cat06Social",
                "Utilities" => "Cat07Utilities",
                "Education" => "Cat08Education",
                "Finance" => "Cat09Finance",
                "Health" => "Cat10Health",
                _ => null,
            };
            if (dataKey != null && Application.Current.TryFindResource(dataKey) is string dataLabel)
                return dataLabel;

            return null;
        }

        public bool IsFeaturedVisible =>
            _selectedCategory == "Discover"
            && string.IsNullOrEmpty(_searchQuery)
            && _selectedTag == null;
        public bool IsEmpty => Apps.Count == 0;
        public int TotalCount => _repo.GetAllApps().Count;
        public int DownloadedCount => _repo.GetAllApps().Count(a => a.IsDownloaded);

        // ── auth ─────────────────────────────────────────────────────────────
        public IAuthService AuthService => _auth;
        public IThemeService ThemeService => _theme;
        public User? CurrentUser => _auth.CurrentUser;
        public bool IsAdmin => _auth.IsAdmin;

        // ── commands ─────────────────────────────────────────────────────────
        public ICommand DownloadCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand SelectTagCommand { get; }
        public ICommand SwitchLanguageCommand { get; }
        public ICommand RateAppCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleSeeAllCommand { get; }
        public ICommand RestoreDefaultsCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public event EventHandler? LogoutRequested;

        // ── constructor ──────────────────────────────────────────────────────
        public MainViewModel(
            IRepository repo,
            ILocalizationService localization,
            IAuthService auth,
            IThemeService theme
        )
        {
            _repo = repo;
            _localization = localization;
            _auth = auth;
            _theme = theme;

            DownloadCommand = new RelayCommand<App>(app =>
            {
                if (app == null)
                    return;

                var id = app.Id;

                if (app.IsDownloaded)
                {
                    _repo.UninstallApp(id);
                    _undoRedo.Push(
                        new OpenAction(
                            undo: () =>
                            {
                                _repo.DownloadApp(id);
                                RefreshAfterDownload(id);
                            },
                            redo: () =>
                            {
                                _repo.UninstallApp(id);
                                RefreshAfterDownload(id);
                            }
                        )
                    );
                }
                else
                {
                    _repo.DownloadApp(id);
                    _undoRedo.Push(
                        new GetAction(
                            undo: () =>
                            {
                                _repo.UninstallApp(id);
                                RefreshAfterDownload(id);
                            },
                            redo: () =>
                            {
                                _repo.DownloadApp(id);
                                RefreshAfterDownload(id);
                            }
                        )
                    );
                }

                RefreshAfterDownload(id);
            });

            UndoCommand = new RelayCommand(
                () =>
                {
                    _undoRedo.Undo();
                    Reload();
                },
                () => _undoRedo.CanUndo
            );

            RedoCommand = new RelayCommand(
                () =>
                {
                    _undoRedo.Redo();
                    Reload();
                },
                () => _undoRedo.CanRedo
            );

            DeleteCommand = new RelayCommand<Guid>(id =>
            {
                _repo.DeleteApp(id);
                IsDetailOpen = false;
                SelectedApp = null;
                Reload();
            });

            OpenDetailCommand = new RelayCommand<App>(app => SelectedApp = app);
            CloseDetailCommand = new RelayCommand(() =>
            {
                IsDetailOpen = false;
                SelectedApp = null;
                IsCategoryGridOpen = false;
            });

            SelectCategoryCommand = new RelayCommand<string>(cat =>
            {
                if (cat == null)
                    return;
                _selectedTag = null;
                // Единый список строк для всех разделов (решение автора
                // 15.11.2026): сетка карточек для категорий удалена.
                IsCategoryGridOpen = false;
                if (cat == "Categories")
                {
                    IsDetailOpen = false;
                    _selectedCategory = "Categories";
                    _visibleCount = int.MaxValue;
                    RefreshList();
                    OnPropertyChanged(nameof(SelectedCategory));
                    OnPropertyChanged(nameof(SelectedCategoryLabel));
                    OnPropertyChanged(nameof(IsFeaturedVisible));
                }
                else
                {
                    SelectedCategory = cat;
                }
            });

            SelectTagCommand = new RelayCommand<string>(tag =>
            {
                if (tag == null)
                    return;
                _selectedTag = tag;
                _visibleCount = int.MaxValue;
                IsCategoryGridOpen = false;
                IsDetailOpen = false;
                RefreshList();
                OnPropertyChanged(nameof(IsFeaturedVisible));
                OnPropertyChanged(nameof(SelectedCategoryLabel));
            });

            SwitchLanguageCommand = new RelayCommand<string>(lang =>
            {
                if (lang != null)
                    SwitchLanguage(lang);
            });

            RateAppCommand = new RelayCommand<string>(value =>
            {
                if (SelectedApp == null || !int.TryParse(value, out var rating))
                    return;

                if (_auth.CurrentUser == null)
                    return;

                _repo.RateApp(SelectedApp.Id, rating);
                UserRating = rating;
                Reload();
                RefreshSelectedApp(SelectedApp.Id);
            });
            ClearSearchCommand = new RelayCommand(() => SearchQuery = string.Empty);
            RefreshCommand = new RelayCommand(Reload);
            ToggleSeeAllCommand = new RelayCommand(() =>
            {
                _visibleCount = _visibleCount <= 6 ? int.MaxValue : 6;
                RefreshList();
            });
            RestoreDefaultsCommand = new RelayCommand(() =>
            {
                _repo.RestoreDefaults();
                Reload();
            });
            LogoutCommand = new RelayCommand(() =>
            {
                _undoRedo.Clear();
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            });

            Reload();
        }

        // ── public reload (called after add/edit/delete) ─────────────────────
        public void Reload()
        {
            LoadAllTags();
            LoadFeatured();
            RefreshList();
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(DownloadedCount));
        }

        // ── private helpers ──────────────────────────────────────────────────

        private void LoadAllTags()
        {
            var tags = _repo.GetAllApps().SelectMany(a => a.Tags).Distinct().OrderBy(t => t);
            AllTags = new ObservableCollection<string>(tags);
        }

        private void LoadFeatured() =>
            FeaturedApps = new ObservableCollection<App>(
                _repo.GetAllApps().Where(a => a.IsFeatured)
            );

        private void RefreshList()
        {
            var result = BuildQuery();

            if (_visibleCount < int.MaxValue)
                result = result.Take(_visibleCount);

            Apps = new ObservableCollection<App>(result);
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsFeaturedVisible));
        }

        private IEnumerable<App> BuildQuery()
        {
            var all = _repo.GetAllApps();

            // Search takes priority
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                var q = _searchQuery.ToLowerInvariant();
                return all.Where(a =>
                    a.ShortName.ToLowerInvariant().Contains(q)
                    || a.FullName.ToLowerInvariant().Contains(q)
                    || a.Developer.ToLowerInvariant().Contains(q)
                    || a.Category.ToLowerInvariant().Contains(q)
                    || a.Tags.Any(t => t.ToLowerInvariant().Contains(q))
                );
            }

            // Tag filter
            if (_selectedTag != null)
            {
                var tagged = all.Where(a => a.Tags.Contains(_selectedTag));
                return _sortBy == "name"
                    ? tagged.OrderBy(a => a.ShortName)
                    : tagged.OrderByDescending(a => a.Rating);
            }

            // Show all categories alphabetically
            if (_selectedCategory == "Categories")
                return all.OrderBy(a => a.ShortName);

            // Resolve nav key → category string
            NavCategoryMap.TryGetValue(_selectedCategory, out var dbCat);

            // Build filtered query
            IEnumerable<App> query = all;

            if (dbCat != null)
                query = query.Where(a => a.Category == dbCat);

            if (_selectedCategory == "Installed" || _downloadedOnly)
                query = query.Where(a => a.IsDownloaded);

            if (_minPrice > 0)
                query = query.Where(a => a.FinalPrice >= _minPrice);

            if (_maxPrice < 1000)
                query = query.Where(a => a.FinalPrice <= _maxPrice);

            if (_minRating > 0)
                query = query.Where(a => a.Rating >= _minRating);

            return _sortBy switch
            {
                "name" => query.OrderBy(a => a.ShortName),
                "price" => query.OrderBy(a => a.FinalPrice),
                "rating" => query.OrderByDescending(a => a.Rating),
                "newest" => query.OrderByDescending(a => a.ReleaseDate),
                "featured" => query
                    .OrderByDescending(a => a.IsFeatured)
                    .ThenByDescending(a => a.Rating),
                _ => query.OrderByDescending(a => a.IsFeatured).ThenByDescending(a => a.Rating),
            };
        }

        /// <summary>
        /// Сохранить настройки интерфейса в профиле, если пользователь авторизован.
        /// Ошибки записи не должны ломать работу интерфейса.
        /// </summary>
        private void TryPersistPreferences(string? language, string? theme)
        {
            try
            {
                var user = _auth?.CurrentUser;
                if (user == null)
                    return;

                _auth!.UpdatePreferences(
                    language ?? user.Language,
                    theme ?? user.Theme);
            }
            catch
            {
                // профиль мог быть ещё не сохранён в базу — не критично
            }
        }

        private void SwitchLanguage(string lang)
        {
            _localization.SetLanguage(lang);
            CurrentLanguage = lang;

            // Заголовок страницы и категории пересчитываются под новый язык.
            OnPropertyChanged(nameof(SelectedCategoryLabel));
            OnPropertyChanged(nameof(IsFeaturedVisible));

            TryPersistPreferences(lang, null);
            // Rebuild collections so converters re-evaluate with new currency/labels
            Reload();
        }

        private void RefreshAfterDownload(Guid id)
        {
            Reload();
            OnPropertyChanged(nameof(DownloadedCount));

            if (_selectedApp?.Id == id)
            {
                var fresh = _repo.GetAppById(id);
                bool wasOpen = _isDetailOpen;
                _selectedApp = null;
                OnPropertyChanged(nameof(SelectedApp));
                _selectedApp = fresh;
                _isDetailOpen = wasOpen;
                OnPropertyChanged(nameof(SelectedApp));
            }
        }

        /// <summary>Прочитать оценку текущего пользователя для выбранного приложения.</summary>
        private void LoadUserRating()
        {
            if (_selectedApp == null || _auth.CurrentUser == null)
            {
                UserRating = null;
                return;
            }

            UserRating = _repo.GetUserRating(_selectedApp.Id);
        }

        /// <summary>Перечитать выбранное приложение, сохранив открытую панель деталей.</summary>
        private void RefreshSelectedApp(Guid id)
        {
            if (_selectedApp?.Id != id)
                return;

            var fresh = _repo.GetAppById(id);
            if (fresh == null)
                return;

            bool wasOpen = _isDetailOpen;
            _selectedApp = null;
            OnPropertyChanged(nameof(SelectedApp));
            _selectedApp = fresh;
            _isDetailOpen = wasOpen;
            OnPropertyChanged(nameof(SelectedApp));
            IsDetailOpen = wasOpen;
        }
    }
}
