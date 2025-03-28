using UnityEngine;

public class FixedPointConstraint : Constraint
{
    public override void ApplyConstraint(Vector3 movement)
    {
        foreach (Shape shape in GetLinkedShapes())
        {
            shape.MoveToPosition(shape.Position + movement);
        }
    }
}