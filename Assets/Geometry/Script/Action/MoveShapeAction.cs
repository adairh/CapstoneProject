using Manipulator;
using UnityEngine;

namespace Manipulator
{
    public class MoveShapeAction : IUndoableAction
    {
        public string ShapeId { get; }
        public Vector3 OldPosition { get; }
        public Vector3 NewPosition { get; }

        public MoveShapeAction(string shapeId, Vector3 from, Vector3 to)
        {
            ShapeId = shapeId;
            OldPosition = from;
            NewPosition = to;
        }

        public void Undo()
        {
            var shape = ShapeStorage.GetById(ShapeId);

            foreach (var a in ShapeStorage.GetAllShapes())
            {
                Debug.LogError($"[MoveShapeAction-All] {a.ShapeId}");
            }
            
            Debug.LogError($"[MoveShapeAction] {ShapeId}");
            Debug.LogError($"[MoveShapeAction] {shape != null}");
            Debug.LogError($"[MoveShapeAction] {shape.transform.position}");
            Debug.LogError($"[MoveShapeAction] {NewPosition}");
            Debug.LogError($"[MoveShapeAction] {OldPosition}");
            
            shape?.MoveTo(OldPosition, silent: false, queue: false); // 🔥 sửa thành false để update transform
            
        }

        public void Redo()
        {
            var shape = ShapeStorage.GetById(ShapeId);
            if (shape == null) return;

            shape.isInternalMove = true;
            shape.MoveTo(NewPosition, silent: false); // 🔥 sửa thành false
            shape.isInternalMove = false;
        }


    }

}