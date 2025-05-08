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
            shape?.MoveTo(OldPosition, silent: true);
        }

        public void Redo()
        {
            Shape shape = ShapeStorage.GetById(ShapeId);
            if (shape == null) return;

            shape.isInternalMove = true;
            shape.MoveTo(NewPosition, silent: true);
            shape.isInternalMove = false;
        }

    }

}