using System.Collections.Generic;
using System.Linq;

namespace Project.Services
{
    /// <summary>
    /// Сервис управления историей действий (Undo/Redo).
    /// Хранит стек действий с ограничением размера (по умолчанию 10).
    /// При logout очищается — история одного пользователя не видна другому.
    /// </summary>
    public class UndoRedoService : IUndoRedoService
    {
        private const int MaxActions = 10;

        private readonly Stack<IUndoRedoAction> _undoStack = new();
        private readonly Stack<IUndoRedoAction> _redoStack = new();

        /// <summary>Есть ли действия для отмены (Undo).</summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>Есть ли действия для повтора (Redo).</summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Сохранить уже выполненное действие в историю.
        /// Caller сам вызывает repo-метод ДО Push.
        /// </summary>
        /// <param name="action">Действие с реализацией Undo/Redo.</param>
        public void Push(IUndoRedoAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear();

            if (_undoStack.Count > MaxActions)
            {
                var items = _undoStack.ToArray(); // [новый, ..., старый]
                _undoStack.Clear();
                for (int i = MaxActions - 1; i >= 0; i--)
                    _undoStack.Push(items[i]);
            }
        }

        /// <summary>
        /// Отменить последнее действие (Undo).
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
                return;
            var action = _undoStack.Pop();
            action.Undo();
            _redoStack.Push(action);
        }

        /// <summary>
        /// Повторить отменённое действие (Redo).
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
                return;
            var action = _redoStack.Pop();
            action.Redo();
            _undoStack.Push(action);
        }

        /// <summary>
                        /// Очистить историю (при logout — данные одного пользователя не видит другой).
                        /// </summary>
                        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
