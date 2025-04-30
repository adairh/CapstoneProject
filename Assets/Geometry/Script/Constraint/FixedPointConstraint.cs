using UnityEngine;

namespace Manipulator
{
    public class FixedPointConstraint : Constraint
    {
        public override void ApplyConstraint(Shape movedShape, Vector3 movement)
        {
            foreach (Shape shape in GetLinkedShapes())
            {
                shape.MoveToPosition(shape.Position + movement);
            }
        }
    }
}