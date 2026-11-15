using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Project.Commands;
using AppStore.Core.Data;

namespace Project.ViewModels
{
    public class AddEditAppViewModel : ViewModelBase
    {
        private readonly IRepository _repo;
        private App _app = null!;
        private bool _isEditMode;

        public App App
        {
            get => _app;
            set => SetField(ref _app, value);
        }
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetField(ref _isEditMode, value);
        }

        public ObservableCollection<string> AvailableCategories { get; } =
            new()
            {
                "Development",
                "Design",
                "Productivity",
                "Games",
                "Media",
                "Social",
                "Utilities",
                "Education",
                "Finance",
                "Health",
            };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }
        public Action? OnSaved { get; set; }
        public Action? OnCancelled { get; set; }
        public Action? OnDeleted { get; set; }

        /// <summary>
        /// Теги приложения одной строкой (через запятую). REPORT-007: тегами
        /// управляет администратор — форма Add/Edit доступна только роли Admin;
        /// разбор и нормализацию выполняет репозиторий (ReplaceTags: trim + distinct).
        /// </summary>
        public string TagsText
        {
            get => string.Join(", ", App.Tags);
            set
            {
                App.Tags = value
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => t.Length > 0)
                    .Distinct()
                    .ToList();
            }
        }

        public AddEditAppViewModel(IRepository repo)
        {
            _repo = repo;
            Reset();
            SaveCommand = new RelayCommand(Save, () => !string.IsNullOrWhiteSpace(App.ShortName));
            CancelCommand = new RelayCommand(() => OnCancelled?.Invoke());
            // REPORT-007: удалять каталог может только администратор — форма
            // доступна только роли Admin, удаление допустимо в режиме правки.
            DeleteCommand = new RelayCommand(Delete, () => IsEditMode && App.Id != Guid.Empty);
        }

        public void LoadExisting(App existing)
        {
            if (existing != null)
            {
                App = Clone(existing);
                IsEditMode = true;
            }
            else
            {
                Reset();
            }
            OnPropertyChanged(nameof(TagsText));
        }

        private void Reset()
        {
            App = new App { ReleaseDate = DateTime.Now };
            IsEditMode = false;
        }

        private void Save()
        {
            if (_isEditMode)
                _repo.UpdateApp(App);
            else
                _repo.AddApp(App);
            OnSaved?.Invoke();
        }

        private void Delete()
        {
            _repo.DeleteApp(App.Id);
            OnDeleted?.Invoke();
        }

        private static App Clone(App s) =>
            new()
            {
                Id = s.Id,
                ShortName = s.ShortName,
                FullName = s.FullName,
                Description = s.Description,
                Developer = s.Developer,
                Category = s.Category,
                Rating = s.Rating,
                RatingCount = s.RatingCount,
                Price = s.Price,
                Version = s.Version,
                SizeMB = s.SizeMB,
                Country = s.Country,
                AgeRating = s.AgeRating,
                Color = s.Color,
                IsFeatured = s.IsFeatured,
                IsDownloaded = s.IsDownloaded,
                IsInStock = s.IsInStock,
                DownloadCount = s.DownloadCount,
                DiscountPercent = s.DiscountPercent,
                ReleaseDate = s.ReleaseDate,
                Tags = new(s.Tags),
            };
    }
}
