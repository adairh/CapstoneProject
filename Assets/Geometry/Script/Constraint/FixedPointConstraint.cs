using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    /// <summary>
    /// Dữ liệu cố định Point gắn với Shape.
    /// </summary>
    [Serializable]
    public class FixedPointConstraintData : ConstraintData
    {
        public string PointId;
        public string TargetShapeId;
        //public List<Shape> BelongTo = new();

        public override void Restore()
        {
            var point = ShapeStorage.GetById(PointId) as Point;
            var shape = ShapeStorage.GetById(TargetShapeId);
            if (point == null || shape == null) return;

            var constraint = point.gameObject.AddComponent<FixedPointConstraint>();
            constraint.Owner = point;
            constraint.AddDepend(point, shape);
        }
    }

    public class FixedPointConstraint : Constraint
    {
        public Point Owner { get; set; }
        private readonly List<Shape> dependencies = new();

        public void AddDepend(Point point, Shape shape)
        {
            if (!dependencies.Contains(shape))
            {
                dependencies.Add(shape);
                shape.OnChanged += OnShapeChanged;
                ApplyConstraint(shape, Vector3.zero);
            }
        }

        public override bool HasShape(Shape shape) => dependencies.Contains(shape);

        public override void ApplyConstraint(Shape changedShape, Vector3 delta)
        {
            if (Owner == null || changedShape == null) return;
            Vector3 oldPos = Owner.transform.position;
            Vector3 newPos = oldPos + delta;
            Owner.MoveTo(newPos, silent: true);
        }

        public override ConstraintData Serialize()
        {
            return new FixedPointConstraintData
            {
                Type = "FixedPoint",
                ConstraintId = ConstraintId,
                PointId = Owner.ShapeId,
                TargetShapeId = dependencies.Count > 0 ? dependencies[0].ShapeId : ""
            };
        }

        public override IEnumerable<Shape> GetRelatedShapes() => dependencies;

        public override void Cleanup()
        {
            foreach (var shape in dependencies)
            {
                shape.OnChanged -= OnShapeChanged;
            }
            dependencies.Clear();
        }

        private void OnShapeChanged(Shape shape)
        {
            ApplyConstraint(shape, Vector3.zero);
        }
    }
} 
