using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RelativePointConstraint : Constraint
    {
        public Point Owner;

        // NEW: for Polygon triangle mapping
        private int indexA, indexB, indexC;
        private float t, u, v;
        private Shape target;
        private RelativeTargetType type;
        private bool usePolygonTriangle;
        
        public float T
        {
            get => t;
            set
            {
                t = Mathf.Clamp01(value);
                UpdatePosition();
            }
        }

        public Segment TargetSegment => target as Segment;


        public void SetTarget(Shape shape, RelativeTargetType type, float t, float u, float v)
        {
            target = shape;
            this.type = type;
            this.t = t;
            this.u = u;
            this.v = v;
            shape.OnChanged += OnTargetChanged;
            UpdatePosition(); // áp dụng ngay lúc bind
        }

        // NEW: for polygon triangle-based constraint
        public void SetPolygonTriangleTarget(Polygon polygon, int ia, int ib, int ic, float u, float v)
        {
            target = polygon;
            type = RelativeTargetType.Plane;
            indexA = ia;
            indexB = ib;
            indexC = ic;
            this.u = u;
            this.v = v;
            usePolygonTriangle = true;
            polygon.OnChanged += OnTargetChanged;
            UpdatePosition();
        }

        private void OnTargetChanged(Shape shape)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (target == null || Owner == null) return;

            if (type == RelativeTargetType.Segment && target is Segment seg)
            {
                var a = seg.StartPoint.transform.position;
                var b = seg.EndPoint.transform.position;
                var pos = Vector3.Lerp(a, b, t);
                Owner.MoveTo(pos, true);
            }
            else if (type == RelativeTargetType.Plane)
            {
                if (usePolygonTriangle && target is Polygon poly)
                {
                    var pts = poly.Points;
                    if (pts.Count <= Mathf.Max(indexA, indexB, indexC)) return;

                    var a = pts[indexA].transform.position;
                    var b = pts[indexB].transform.position;
                    var c = pts[indexC].transform.position;
                    var pos = a + u * (b - a) + v * (c - a);
                    Owner.MoveTo(pos, true);
                }
                else if (target is PlaneShape plane)
                {
                    var pts = plane.PivotPoints;
                    if (pts.Count < 3) return;

                    var a = pts[0].transform.position;
                    var b = pts[1].transform.position;
                    var c = pts[2].transform.position;
                    var pos = a + u * (b - a) + v * (c - a);
                    Owner.MoveTo(pos, true);
                }
            }
        }

        public override void ApplyConstraint(Shape changedShape, Vector3 delta)
        {
            UpdatePosition();
        }

        public override IEnumerable<Shape> GetRelatedShapes()
        {
            return new[] { target };
        }

        public override bool HasShape(Shape shape)
        {
            return shape == target;
        }

        public override ConstraintData Serialize()
        {
            return new RelativePointConstraintData
            {
                ConstraintId = ConstraintId,
                PointId = Owner.ShapeId,
                TargetShapeId = target.ShapeId,
                TargetType = type,
                T = t,
                U = u,
                V = v,
                IndexA = indexA,
                IndexB = indexB,
                IndexC = indexC,
                Type = "RelativePoint"
            };
        }

        public override void Cleanup()
        {
            if (target != null)
                target.OnChanged -= OnTargetChanged;
        }
    }
}