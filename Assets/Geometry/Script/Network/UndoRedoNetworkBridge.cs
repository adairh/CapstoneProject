using UnityEngine;
using Unity.Netcode; 
using System;
using System.Collections.Generic;

namespace Manipulator
{
    public class UndoRedoNetworkBridge : NetworkBehaviour
    {
        public static UndoRedoNetworkBridge Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void DoAndBroadcast(IUndoableAction action, bool queue = true)
        {
            if (queue) UndoRedoManager.Instance.Do(action);

            //.LogError($"[DoAndBroadcast] IsHost: {IsHost}");
            if (IsHost)
            {
                switch (action)
                {
                    case MoveShapeAction msa:
                        BroadcastMoveShapeClientRpc(msa.ShapeId, msa.NewPosition);
                        break;
                    case DeleteShapeAction dsa:
                        BroadcastDeleteShapeClientRpc(dsa.GetShape().ShapeId);
                        break;
                    case CreateShapeAction csa:
                        //Debug.LogError($"[DoAndBroadcast] {csa.data.Id}");
                        BroadcastCreateShapeClientRpc(JsonUtility.ToJson(csa.data));
                        break;
                    case CreateShapeBatchAction csba:
                        var wrapper = new ShapeDataListWrapper { Shapes = csba.shapeDataList };
                        string json = JsonUtility.ToJson(wrapper);
                        BroadcastCreateShapeBatchClientRpc(json);
                        break;
                    case MultiMoveShapeAction mma:
                        foreach (var move in mma.GetSubActions())
                            BroadcastMoveShapeClientRpc(move.ShapeId, move.NewPosition);
                        break;


                    default:
                        //Debug.LogError($"[DoAndBroadcast] Type: {action.GetType()}");
                        break;
                }
            }
        }

        [ClientRpc]
        public void BroadcastCreateShapeBatchClientRpc(string json)
        {
            if (IsHost) return; // tránh host nhận lại

            var wrapper = JsonUtility.FromJson<ShapeDataListWrapper>(json);

            foreach (var data in wrapper.Shapes)
            {
                var shape = ShapeFactory.CreateFromData(data);
                if (shape == null)
                {
                    //Debug.LogError($"[ClientBatchCreate] Failed to create shape of type {data.Type}");
                    continue;
                }

                shape.ShapeId = data.Id;
                shape.Deserialize(data);
                ShapeStorage.Register(shape);
            }
        }


        [Serializable]
        public class ShapeDataListWrapper
        {
            public List<ShapeData> Shapes;
        }

        
        [ClientRpc]
        private void BroadcastMoveShapeClientRpc(string shapeId, Vector3 pos)
        {
            if (IsHost) return;

            //Debug.LogError($"[BroadcastMoveShapeClientRpc] shapeId={shapeId} to={pos}");

            var shape = ShapeStorage.GetById(shapeId);
            if (shape == null)
            {
                //Debug.LogError($"[BroadcastMoveShapeClientRpc] shape NOT found in ShapeStorage");
                return;
            }

            shape.MoveTo(pos, silent: true);
        }


        [ClientRpc]
        public void BroadcastDeleteShapeClientRpc(string shapeId)
        {
            if (IsHost) return;
            var shape = ShapeStorage.GetById(shapeId);
            if (shape != null)
                shape.DestroyShape();
        }

        [ClientRpc]
        private void BroadcastCreateShapeClientRpc(string json)
        {
            //Debug.LogError($"[BroadcastCreateShapeClientRpc] {IsHost}");
            if (IsHost) return;
            var data = JsonUtility.FromJson<ShapeData>(json);
            ShapeFactory.CreateFromData(data);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RequestUndoServerRpc()
        {
            UndoRedoManager.Instance.Undo();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRedoServerRpc()
        {
            UndoRedoManager.Instance.Redo();
        }

        public void SpawnFromData(ShapeData data, Action<Shape> onCreated)
        {
            CreateShapeNetworked(data, out Shape shape);
            onCreated?.Invoke(shape);
        }

        
        public void CreateShapeNetworked(ShapeData data, out Shape shape)
        {
            shape = ShapeFactory.CreateFromData(data); // Factory sẽ gán đúng prefab, type, component
            if (shape == null)
            {
                //Debug.LogError($"[CreateShapeNetworked] Failed to create shape for type={data.Type}");
                return;
            }

            shape.ShapeId = data.Id; // Đảm bảo giữ ID gốc để match Undo/Redo
            shape.Deserialize(data);
            ShapeStorage.Register(shape);

            if (shape.TryGetComponent(out NetworkObject netObj))
            {
                if (!netObj.IsSpawned)
                    netObj.Spawn();
            }
            else
            {
                Debug.LogWarning($"[CreateShapeNetworked] Created shape {shape.ShapeId} has no NetworkObject");
            }
        }

        
    }
 
}
