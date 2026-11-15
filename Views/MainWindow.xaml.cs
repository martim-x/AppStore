using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Project.Commands;
using AppStore.Core.Data;
using Project.ViewModels;

namespace Project.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel Vm => (MainViewModel)DataContext;
        private readonly IServiceProvider _services;

        // Expose services for child controls (AppDetailView needs it for EditDeleteDialog)
        internal IServiceProvider Services => _services;

        public ObservableCollection<string> EventLog { get; } = new();

        public MainWindow(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            InitializeComponent();

            try
            {
                var uri = new Uri("pack://application:,,,/Resources/Cursors/arrow.cur");
                var stream = Application.GetResourceStream(uri)?.Stream;
                if (stream != null)
                    Cursor = new Cursor(stream);
            }
            catch { }
        }

        private void Log(string message)
        {
            EventLog.Add($"{DateTime.Now:HH:mm:ss} | {message}");
            Debug.WriteLine(message);
        }

        private void Banner_PreviewBannerClick(object sender, RoutedEventArgs e)
        {
            Log("PreviewBannerClick (Tunneling) -> MainWindow");
        }

        private void Banner_BannerLoaded(object sender, RoutedEventArgs e)
        {
            Log("BannerLoaded (Direct) -> MainWindow");
        }

        private void Banner_BannerClicked(object sender, RoutedEventArgs e)
        {
            Log("BannerClicked (Bubbling) -> MainWindow");
        }

        private void AppCard_PreviewCardClick(object sender, RoutedEventArgs e)
        {
            Log("PreviewCardClick (Tunneling) -> MainWindow");
        }

        private void AppCard_CardClicked(object sender, RoutedEventArgs e)
        {
            Log("CardClicked (Bubbling) -> MainWindow");
        }

        private void AppRow_RowClicked(object sender, RoutedEventArgs e)
        {
            Log("AppRow RowClicked (Bubbling) -> MainWindow");
        }

        private void DownloadApp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is App;
        }

        private void DownloadApp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is App app)
            {
                Log($"RoutedUICommand DownloadApp executed for: {app.FullName}");

                if (Vm.DownloadCommand.CanExecute(app))
                    Vm.DownloadCommand.Execute(app);
            }
        }

        private void OpenDetails_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is App;
        }

        private void OpenDetails_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is App app)
            {
                Log($"RoutedUICommand OpenDetails executed for: {app.FullName}");

                if (Vm.OpenDetailCommand.CanExecute(app))
                    Vm.OpenDetailCommand.Execute(app);
            }
        }

        private void OnSearchGotFocus(object sender, RoutedEventArgs e)
        {
            SearchBorder.BorderThickness = new Thickness(1.5);
            SearchBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("SearchFocusBrush");
        }

        private void OnSearchLostFocus(object sender, RoutedEventArgs e)
        {
            SearchBorder.BorderThickness = new Thickness(0);
        }

        private void OnClearSearch(object sender, MouseButtonEventArgs e)
        {
            Vm.ClearSearchCommand.Execute(null);
        }

        private void OnFilterClick(object sender, RoutedEventArgs e)
        {
            FilterPopup.PlacementTarget = (UIElement)sender;
            FilterPopup.Placement = PlacementMode.Bottom;
            FilterPopup.IsOpen = true;
        }

        // REPORT-010: панель фильтров открывается под кнопкой, а её правый
        // край совпадает с правым краем главного окна (решение автора 15.11.2026).
        private void OnFilterPopupOpened(object sender, EventArgs e)
        {
            if (FilterPopup.PlacementTarget is FrameworkElement target
                && Window.GetWindow(target) is Window win)
            {
                var leftInWin = target.TranslatePoint(new Point(0, 0), win).X;
                FilterPopup.HorizontalOffset =
                    win.ActualWidth - FilterPopup.ActualWidth - leftInWin;
            }
        }

        private void OnAddAppClick(object sender, RoutedEventArgs e)
        {
            var dlg = _services.GetService<AddEditWindow>() ?? new AddEditWindow(_services);
            dlg.Owner = this; // REPORT-010: по центру главного окна

            if (dlg.ShowDialog() == true)
                Vm.RefreshCommand.Execute(null);
        }

        private void OnResetFilters(object sender, RoutedEventArgs e)
        {
            Vm.MinRating = 0;
            Vm.MaxPrice = AppStore.Core.Configuration.Settings.Instance.MaxPriceFilter;
            Vm.MinPrice = 0;
            Vm.DownloadedOnly = false;
            FilterPopup.IsOpen = false;
        }

        private void OnRestoreData(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = false;

            var dlg = new ConfirmDialog(
                (string)Application.Current.Resources["RestoreConfirmTitle"],
                (string)Application.Current.Resources["RestoreConfirmMsg"],
                (string)Application.Current.Resources["RestoreBtn"],
                isDanger: false
            )
            {
                Owner = this,
            };

            if (dlg.ShowDialog() == true)
                Vm.RestoreDefaultsCommand.Execute(null);
        }

        private void OnProfileClick(object sender, RoutedEventArgs e)
        {
            var popup = _services.GetService<ProfilePopup>() ?? new ProfilePopup(_services);
            popup.Initialize(this);
            popup.Show();

            popup.Closed += (_, __) =>
            {
                if (popup.LoggedOut)
                    Vm.LogoutCommand.Execute(null);
            };
        }

        private void OnCategoryTileClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MainViewModel.CategoryItem cat)
                Vm.SelectCategoryCommand.Execute(cat.Name);
        }

        // Плитки категорий: число колонок подстраивается под ширину рабочей
        // области (мин. ширина плитки 220 + отступ 12), чтобы при сжатии окна
        // стрелка не уезжала за край плитки.
        private void OnCategoriesGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is UniformGrid grid)
                grid.Columns = Math.Max(1, (int)(e.NewSize.Width / 232));
        }
    }
}