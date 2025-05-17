using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    /// <summary>
    ///     MonoBehaviour constraint gắn lên một Point pivot để giữ góc cố định giữa
    ///     hai Segment. Phản hồi khi kéo pivot, segment hoặc endpoint.
    /// </summary>
    [Serializable]
    public class AngleConstraintData : ConstraintData
    {
        public string PointAId;
        public string VertexId;
        public string PointBId;

        public override void Restore()
        {
            var A = ShapeStorage.GetById(PointAId) as Point;
            var B = ShapeStorage.GetById(VertexId) as Point;
            var C = ShapeStorage.GetById(PointBId) as Point;
            if (A == null || B == null || C == null) return;

            var constraint = B.gameObject.AddComponent<AngleConstraint>();
            constraint.Initialize(A, B, C);
        }
    }

    public class AngleConstraint : Constraint
    {
        private Point A, B, C;
        private float initialAngle;

        public void Initialize(Point pointA, Point vertex, Point pointB)
        {
            A = pointA;
            B = vertex;
            C = pointB;

            initialAngle = GetAngle();

            A.OnChanged += OnShapeChanged;
            B.OnChanged += OnShapeChanged;
            C.OnChanged += OnShapeChanged;

            ApplyConstraint(null, Vector3.zero);
        }

        public override bool HasShape(Shape shape)
        {
            return shape == A || shape == B || shape == C;
        }

        public override void ApplyConstraint(Shape changedShape, Vector3 delta)
        {
            var currentAngle = GetAngle();
            var deltaAngle = currentAngle - initialAngle;
            // TODO: Apply correction logic if necessary
        }

        public override ConstraintData Serialize()
        {
            return new AngleConstraintData
            {
                Type = "Angle",
                ConstraintId = ConstraintId,
                PointAId = A.ShapeId,
                VertexId = B.ShapeId,
                PointBId = C.ShapeId
            };
        }

        public override IEnumerable<Shape> GetRelatedShapes()
        {
            yield return A;
            yield return B;
            yield return C;
        }

        public override void Cleanup()
        {
            A.OnChanged -= OnShapeChanged;
            B.OnChanged -= OnShapeChanged;
            C.OnChanged -= OnShapeChanged;
        }

        private float GetAngle()
        {
            var ab = A.transform.position - B.transform.position;
            var cb = C.transform.position - B.transform.position;
            return Vector3.Angle(ab, cb);
        }

        private void OnShapeChanged(Shape shape)
        {
            ApplyConstraint(shape, Vector3.zero);
        }
    }
}