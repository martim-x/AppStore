using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AppStore.Core.Data;
using Project.ViewModels;

namespace Project.Views
{
    public partial class AddEditWindow : Window
    {
        private readonly IServiceProvider _services;

        public AddEditWindow(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            InitializeComponent();

            var repository = _services.GetRequiredService<IRepository>();
            var vm = new AddEditAppViewModel(repository);
            vm.OnSaved = () =>
            {
                DialogResult = true;
                Close();
            };
            vm.OnCancelled = () =>
            {
                DialogResult = false;
                Close();
            };
            DataContext = vm;
            Title = "Add New App";
        }

        public void Initialize(App existing)
        {
            if (DataContext is AddEditAppViewModel vm)
            {
                vm.LoadExisting(existing);
                Title = existing != null ? "Edit App" : "Add New App";
            }
        }
    }
}