using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project.Commands
{
    /// <summary>
    /// Команда с асинхронным исполнением. Повторные клики во время выполнения
    /// блокируются, чтобы не порождать параллельные операции.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isRunning;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CanExecute(object? parameter) =>
            !_isRunning && (_canExecute?.Invoke() ?? true);

        public async void Execute(object? parameter)
        {
            if (_isRunning) return;

            IsRunning = true;
            try
            {
                await _execute().ConfigureAwait(true);
            }
            finally
            {
                IsRunning = false;
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
