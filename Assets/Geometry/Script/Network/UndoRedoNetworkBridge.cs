using UnityEngine;
using Unity.Netcode; 
using System;

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

        public void DoAndBroadcast(IUndoableAction action)
        {
            UndoRedoManager.Instance.Do(action);

            if (IsServer)
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
                        BroadcastCreateShapeClientRpc(JsonUtility.ToJson(csa.Data));
                        break;
                }
            }
        }

        [ClientRpc]
        private void BroadcastMoveShapeClientRpc(string shapeId, Vector3 pos)
        {
            if (IsServer) return;
            var shape = ShapeStorage.GetById(shapeId);
            shape?.MoveTo(pos, silent: true);
        }

        [ClientRpc]
        private void BroadcastDeleteShapeClientRpc(string shapeId)
        {
            if (IsServer) return;
            var shape = ShapeStorage.GetById(shapeId);
            if (shape != null)
                shape.DestroyShape();
        }

        [ClientRpc]
        private void BroadcastCreateShapeClientRpc(string json)
        {
            if (IsServer) return;
            var data = JsonUtility.FromJson<ShapeData>(json);
            ShapeFactory.CreateFromData(data);
        }
    }
 
}
