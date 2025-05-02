using System.Collections.Generic;

namespace Manipulator
{
    public interface IUndoableCommand
    {
        void Execute();   // Thực thi lệnh (Redo)
        void Undo();      // Hoàn tác lệnh
    }

    public class UndoManager
    {
        private static UndoManager _instance;
        public static UndoManager Instance => _instance ??= new UndoManager();

        private readonly Stack<IUndoableCommand> _undoStack = new();
        private readonly Stack<IUndoableCommand> _redoStack = new();

        public void Do(IUndoableCommand cmd)
        {
            cmd.Execute();
            _undoStack.Push(cmd);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;
            var cmd = _redoStack.Pop();
            cmd.Execute();
            _undoStack.Push(cmd);
        }
    }

}