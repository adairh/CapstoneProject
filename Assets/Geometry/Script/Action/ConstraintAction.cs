using UnityEngine;

namespace Manipulator
{
    public class ConstraintAction : IUndoableAction
    {
        private readonly Point point;
        private readonly Vector3 oldPos, newPos;

        public ConstraintAction(Point point, Vector3 oldPos, Vector3 newPos)
        {
            this.point = point;
            this.oldPos = oldPos;
            this.newPos = newPos;
        }

        public void Undo() => point.MoveTo(oldPos, silent: true);
        public void Redo() => point.MoveTo(newPos, silent: true);
    }

}