using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class CreateShapeBatchAction : IUndoableAction
    {
        private readonly List<ShapeData> shapeDataList;
        private readonly List<Shape> createdShapes = new();

        public Action<Shape> OnShapeSpawned; // 👈 ADD THIS LINE

        public CreateShapeBatchAction(List<ShapeData> shapeDataList)
        {
            this.shapeDataList = shapeDataList;
        }

        public void Redo()
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
        }


        public void Undo()
        {
            foreach (var shape in createdShapes)
                shape.DestroyShape();
            createdShapes.Clear();
        }
    }
    
}

