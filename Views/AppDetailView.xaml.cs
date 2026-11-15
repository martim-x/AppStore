using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Project.ViewModels;

namespace Project.Views
{
    public partial class AppDetailView : UserControl
    {
        public AppDetailView() => InitializeComponent();

        private void OnEditDeleteClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm || vm.SelectedApp == null)
                return;

            var window = Window.GetWindow(this);
            var services = (window as MainWindow)?.Services;
            if (services == null)
                return;

            // REPORT-010: промежуточный диалог Edit/Delete убран — кнопка
            // «Изменить» сразу открывает окно редактирования; удаление
            // выполняется кнопкой в нижней части этой формы.
            var dlg = services.GetService<AddEditWindow>() ?? new AddEditWindow(services);
            dlg.Initialize(vm.SelectedApp);
            dlg.Owner = window;

            if (dlg.ShowDialog() == true)
                vm.RefreshCommand.Execute(null);
        }
    }
}
