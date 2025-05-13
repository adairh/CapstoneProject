using System;

namespace Manipulator
{
    public enum RelativeTargetType { Segment, Plane }

    [Serializable]
    public class RelativePointConstraintData : ConstraintData
    {
        public string PointId;
        public string TargetShapeId;
        public RelativeTargetType TargetType;
        public float T;     // nếu là segment
        public float U, V;  // nếu là plane
        public int IndexA, IndexB, IndexC; // nếu là polygon tam giác con

        public override void Restore()
        {
            var point = ShapeStorage.GetById(PointId) as Point;
            var shape = ShapeStorage.GetById(TargetShapeId);
            if (point == null || shape == null) return;

            var constraint = point.gameObject.AddComponent<RelativePointConstraint>();
            constraint.Owner = point;

            if (TargetType == RelativeTargetType.Segment)
            {
                constraint.SetTarget(shape, TargetType, T, 0, 0);
            }
            else if (shape is PlaneShape || (shape is Polygon && IndexA >= 0 && IndexB >= 0 && IndexC >= 0))
            {
                if (shape is Polygon polygon)
                    constraint.SetPolygonTriangleTarget(polygon, IndexA, IndexB, IndexC, U, V);
                else
                    constraint.SetTarget(shape, TargetType, 0, U, V);
            }
        }
    }
}