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

        public Action<Shape> OnShapeSpawned;

        public CreateShapeBatchAction(List<ShapeData> shapeDataList)
        {
            this.shapeDataList = shapeDataList;
        }

        private bool isPreview = false;

        public void Redo(bool preview)
        {
            isPreview = preview;
            Redo();
        }

        public void Redo()
        {
            if (!UndoRedoNetworkBridge.Instance.IsHost) return;

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

            string batchJson = JsonUtility.ToJson(new ShapeDataListWrapper { list = shapeDataList });
            UndoRedoNetworkBridge.Instance.BroadcastCreateShapeBatchClientRpc(batchJson);

            if (isPreview)
            {
                // Replace preview in queue with actual on commit
                UndoRedoManager.Instance.ReplaceLast(this);
            }
            else
            {
                UndoRedoManager.Instance.Do(this);
            }

            isPreview = false;
        }

        public void Undo()
        {
            foreach (var shape in createdShapes)
                shape.DestroyShape();
            createdShapes.Clear();
        }
    }
}
