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
        public readonly List<Shape> createdShapes = new();
        public readonly List<ShapeData> shapeDataList;

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
                    UndoRedoNetworkBridge.Instance.SpawnFromData(data, shape =>
                    {
                        if (shape == null)
                            //Debug.LogError($"[CreateShapeBatchAction] Failed to create shape of type {data.Type}");
                            return;

                        shape.ShapeId = data.Id;
                        shape.Deserialize(data);
                        ShapeStorage.Register(shape);

                        createdShapes.Add(shape);
                        OnShapeSpawned?.Invoke(shape);
                    });

                // Gửi xuống client
                var batchJson = JsonUtility.ToJson(new ShapeDataListWrapper { list = shapeDataList });
                UndoRedoNetworkBridge.Instance.BroadcastCreateShapeBatchClientRpc(batchJson);
            }
        }


        public void Undo()
        {
            foreach (var shape in createdShapes)
            {
                if (shape is Point pt)
                {
                    var referenceCount = 0;
                    foreach (var s in ShapeStorage.GetAllShapes())
                        if (s is Segment seg && (seg.StartPoint == pt || seg.EndPoint == pt))
                            referenceCount++;
                    Debug.LogError($"[UNDO] Ref {pt.ShapeId} - {referenceCount}");
                }

                UndoRedoNetworkBridge.Instance.BroadcastDeleteShapeClientRpc(shape.ShapeId);
                shape.DestroyShape();
            }

            createdShapes.Clear();
        }
    }
}