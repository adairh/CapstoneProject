using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    [Serializable]
    public class ShapeDataListWrapper
    {
        public List<ShapeData> list;
    }

    public class CreateShapeBatchAction : IUndoableAction
    {
        public readonly List<ShapeData> shapeDataList;
        private readonly List<Shape> createdShapes = new();

        public Action<Shape> OnShapeSpawned; // 👈 ADD THIS LINE

        public CreateShapeBatchAction(List<ShapeData> shapeDataList)
        {
            this.shapeDataList = shapeDataList;
        }

        public void Redo()
        {
            if (UndoRedoNetworkBridge.Instance.IsHost)
            {
                foreach (var data in shapeDataList)
                {
                    UndoRedoNetworkBridge.Instance.SpawnFromData(data, shape =>
                    {
                        if (shape == null)
                        {
                            Debug.LogError($"[CreateShapeBatchAction] Failed to create shape of type {data.Type}");
                            return;
                        }

                        shape.ShapeId = data.Id;
                        shape.Deserialize(data);
                        ShapeStorage.Register(shape);

                        createdShapes.Add(shape);
                        OnShapeSpawned?.Invoke(shape);
                    });
                }

                // Gửi xuống client
                string batchJson = JsonUtility.ToJson(new ShapeDataListWrapper { list = shapeDataList });
                UndoRedoNetworkBridge.Instance.BroadcastCreateShapeBatchClientRpc(batchJson);
            }
        }


        public void Undo()
        {
            foreach (var shape in createdShapes)
                shape.DestroyShape();
            createdShapes.Clear();
        }
    }
}