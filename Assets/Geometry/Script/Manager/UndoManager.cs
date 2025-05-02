// UndoManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Manipulator
{
    public interface IUndoableAction
    {
        void Execute();
        void Undo();
    }

    [RequireComponent(typeof(NetworkObject))]
    public class UndoManager : NetworkBehaviour
    {
        public static UndoManager Instance { get; private set; }

        private readonly Stack<IUndoableAction> _undo = new();
        private readonly Stack<IUndoableAction> _redo = new();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            // Only the server actually needs to Spawn() this scene object.
            if (IsServer && GetComponent<NetworkObject>() == null)
                GetComponent<NetworkObject>().Spawn();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftAlt))
                Undo();
            if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftAlt))
                Redo();
        }

        public void Do(IUndoableAction action)
        {
            action.Execute();
            _undo.Push(action);
            _redo.Clear();
        }

        public void Undo()
        {
            if (_undo.Count == 0) return;
            var action = _undo.Pop();
            action.Undo();
            _redo.Push(action);
        }

        public void Redo()
        {
            if (_redo.Count == 0) return;
            var action = _redo.Pop();
            action.Execute();
            _undo.Push(action);
        }

        /// <summary>
        /// Called on client to ask server to despawn the wrapper NetworkObject.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void DespawnWrapperServerRpc(ulong networkObjectId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
            {
                netObj.Despawn(true);
            }
        }
        
        /// <summary>
        /// ServerRpc: server despawn wrapper rồi broadcast xuống clients danh sách IDs dạng CSV.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UndoShapesServerRpc(ulong wrapperNetworkObjectId, string shapeIdsCsv)
        {
            // 1) Despawn network wrapper
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(wrapperNetworkObjectId, out var netObj))
            {
                netObj.Despawn(true);
            }

            // 2) Gửi cho các client khác (và cả host) xóa shapes
            DestroyShapesClientRpc(shapeIdsCsv);
        }

        /// <summary>
        /// ClientRpc: nhận string CSV, tách ra và Destroy từng Shape.
        /// </summary>
        [ClientRpc]
        private void DestroyShapesClientRpc(string shapeIdsCsv)
        {
            if (string.IsNullOrEmpty(shapeIdsCsv)) return;

            foreach (var id in shapeIdsCsv.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries))
            {
                var s = ShapeStorage.GetShapeByID(id);
                if (s != null)
                    s.Destroy();
            }
        }
    }
}
