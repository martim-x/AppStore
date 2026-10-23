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

            var dlg = new EditDeleteDialog(vm, services) { Owner = window };
            dlg.ShowDialog();
        }
    }
}
