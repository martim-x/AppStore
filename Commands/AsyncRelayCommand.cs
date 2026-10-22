using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project.Commands
{
    /// <summary>
    /// Команда с асинхронным исполнением. Повторные клики во время выполнения
    /// блокируются, чтобы не порождать параллельные операции.
    /// Пойманное исключение передаётся в onError (если задан) либо в Debug-лог —
    /// failures не глотаются молча.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private readonly Action<Exception>? _onError;
        private bool _isRunning;

        /// <param name="execute">Асинхронная логика команды.</param>
        /// <param name="canExecute">Предикат доступности команды.</param>
        /// <param name="onError">
        /// Обработчик ошибки (например, показать статус для пользователя).
        /// Если не задан, исключение пишется в Debug-лог.
        /// </param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _onError = onError;
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
            catch (Exception ex)
            {
                // Не роняем приложение из-за необработанного исключения в async void,
                // но и не глотаем его молча: логируем и/или отдаём подписчику.
                _onError?.Invoke(ex);
                Debug.WriteLine(ex, "AsyncRelayCommand failed");
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
