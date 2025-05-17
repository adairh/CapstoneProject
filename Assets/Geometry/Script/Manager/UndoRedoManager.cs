// UndoManager.cs

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class UndoRedoManager : MonoBehaviour
    {
        private readonly Stack<IUndoableAction> redoStack = new();

        private readonly Stack<IUndoableAction> undoStack = new();
        public static UndoRedoManager Instance { get; private set; }

        public static bool SuppressRecording { get; set; } = false;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
        }


        public void Do(IUndoableAction action)
        {
            if (SuppressRecording) return;

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
            if (redoStack.Count == 0) return;

            var action = redoStack.Pop();
            action.Redo();
            undoStack.Push(action);

            // 🔧 Add this:
            if (NetworkManager.Singleton.IsHost)
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(action, false); // false: không push lại queue
        }


        public IUndoableAction LastStack()
        {
            if (undoStack.Count == 0) return null;

            var action = undoStack.Peek();

            return action;
        }

        public void ReplaceStack(IUndoableAction action)
        {
            if (undoStack.Count == 0) return;

            undoStack.Pop();
            undoStack.Push(action);
        }
    }
}