// UndoManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Manipulator
{
    public class UndoRedoManager : MonoBehaviour
    {
        public static UndoRedoManager Instance { get; private set; }

        private readonly Stack<IUndoableAction> undoStack = new();
        private readonly Stack<IUndoableAction> redoStack = new();

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
        }

        public void Do(IUndoableAction action)
        {
            action.Redo();
            undoStack.Push(action);
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var action = undoStack.Pop();
                action.Undo();
                redoStack.Push(action);
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var action = redoStack.Pop();
                action.Redo();
                undoStack.Push(action);
            }
        }
    }
}
