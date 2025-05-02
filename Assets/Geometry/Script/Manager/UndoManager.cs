using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Manipulator
{
    public class UndoManager : NetworkBehaviour
    {
        public static UndoManager Instance { get; private set; }
        private readonly Stack<IUndoableAction> _undo = new();
        private readonly Stack<IUndoableAction> _redo = new();

        void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else Destroy(gameObject);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftAlt))
            {
                if (IsServer) PerformUndo();
                else RequestUndoServerRpc();
            }
            if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftAlt))
            {
                if (IsServer) PerformRedo();
                else RequestRedoServerRpc();
            }
        }

        public void Do(IUndoableAction action)
        {
            // luôn chạy trên Server
            if (!IsServer) return;
            action.Execute();
            _undo.Push(action);
            _redo.Clear();
        }

        private void PerformUndo()
        {
            if (_undo.Count == 0) return;
            var a = _undo.Pop();
            a.Undo();
            _redo.Push(a);
        }

        private void PerformRedo()
        {
            if (_redo.Count == 0) return;
            var a = _redo.Pop();
            a.Execute();
            _undo.Push(a);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestUndoServerRpc()
            => PerformUndo();

        [ServerRpc(RequireOwnership = false)]
        private void RequestRedoServerRpc()
            => PerformRedo();

        // RPC để Server despawn wrapper
        [ServerRpc(RequireOwnership = false)]
        public void DespawnWrapperServerRpc(ulong networkObjectId)
        {
            if (NetworkManager.Singleton.SpawnManager
                  .SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
                netObj.Despawn(true);
        }

        // in UndoManager.cs

        /// <summary>
        /// The real “undo batch” routine: despawns the network wrapper
        /// and tells every client to destroy the shapes by ID.
        /// </summary>
        public void ProcessUndoBatch(ulong wrapperNetworkObjectId, string shapeIdsCsv)
        {
            // 1) despawn the wrapper on *this* instance (server)
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(wrapperNetworkObjectId, out var netObj))
            {
                netObj.Despawn(true);
            }

            // 2) broadcast to all clients to destroy the real shapes
            DestroyShapesClientRpc(shapeIdsCsv);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UndoShapesServerRpc(ulong wrapperNetworkObjectId, string shapeIdsCsv)
        {
            // simply forward to the same logic
            ProcessUndoBatch(wrapperNetworkObjectId, shapeIdsCsv);
        }

        [ClientRpc]
        private void DestroyShapesClientRpc(string shapeIdsCsv)
        {
            if (string.IsNullOrEmpty(shapeIdsCsv)) return;
            foreach (var id in shapeIdsCsv.Split(','))
            {
                var s = ShapeStorage.GetShapeByID(id);
                if (s != null) s.Destroy();
            }
        }


        // ServerRpc: khi Redo batch, spawn wrapper lại
        [ServerRpc(RequireOwnership = false)]
        public void SpawnWrapperServerRpc(
            IShapeButton.ShapeType type,
            Vector3 start,
            Vector3 end,
            ulong originalWrapperId)
        {
            var go   = Instantiate(PerformDrawing.Instance.GetShapeNetwork());
            var sync = go.GetComponent<ShapeNetworkSync>();
            sync.shapeType.Value    = (ShapeNetworkSync.ShapeType)type;
            sync.startPoint.Value   = start;
            sync.currentPoint.Value = end;
            sync.isDrawing.Value    = false;
            sync.isFinalized.Value  = true;
            var netObj = go.GetComponent<NetworkObject>();
            netObj.Spawn();
            // (Nếu cần lưu lại _wrapperId mới ở server thì làm tương tự như CreateShapeBatchAction)
        }
    }
}
