// UndoRedoManager.cs
using System.Collections.Generic;
using UnityEngine;


namespace Manipulator
{
    // IUndoableAction.cs
    public interface IUndoableAction
    {
        void Execute();  // thực thi
        void Undo();     // hoàn tác
    }

    
    public class UndoManager : MonoBehaviour
    {
        public static UndoManager Instance { get; private set; }

        private readonly Stack<IUndoableAction> undoStack = new();
        private readonly Stack<IUndoableAction> redoStack = new();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Update()
        {
            // Ctrl+Z để undo, Ctrl+Y để redo
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftAlt))
                Undo();
            if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftAlt))
                Redo();
        }

        public void Do(IUndoableAction action)
        {
            action.Execute();
            undoStack.Push(action);
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;
            var action = undoStack.Pop();
            action.Undo();
            redoStack.Push(action);
        }

        public void Redo()
        {
            if (redoStack.Count == 0) return;
            var action = redoStack.Pop();
            action.Execute();
            undoStack.Push(action);
        }
    }
}