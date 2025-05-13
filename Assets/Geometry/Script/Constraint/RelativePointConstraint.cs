
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{

    public class RelativePointConstraint : Constraint
    {
        public Point Owner;
        private Shape target;
        private RelativeTargetType type;
        private float t, u, v;

        // NEW: for Polygon triangle mapping
        private int indexA, indexB, indexC;
        private bool usePolygonTriangle = false;

        public void SetTarget(Shape shape, RelativeTargetType type, float t, float u, float v)
        {
            this.target = shape;
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
            this.target = polygon;
            this.type = RelativeTargetType.Plane;
            this.indexA = ia;
            this.indexB = ib;
            this.indexC = ic;
            this.u = u;
            this.v = v;
            this.usePolygonTriangle = true;
            polygon.OnChanged += OnTargetChanged;
            UpdatePosition();
        }

        private void OnTargetChanged(Shape shape) => UpdatePosition();

        private void UpdatePosition()
        {
            if (target == null || Owner == null) return;

            if (type == RelativeTargetType.Segment && target is Segment seg)
            {
                Vector3 a = seg.StartPoint.transform.position;
                Vector3 b = seg.EndPoint.transform.position;
                Vector3 pos = Vector3.Lerp(a, b, t);
                Owner.MoveTo(pos, silent: true);
            }
            else if (type == RelativeTargetType.Plane)
            {
                if (usePolygonTriangle && target is Polygon poly)
                {
                    var pts = poly.Points;
                    if (pts.Count <= Mathf.Max(indexA, indexB, indexC)) return;

                    Vector3 a = pts[indexA].transform.position;
                    Vector3 b = pts[indexB].transform.position;
                    Vector3 c = pts[indexC].transform.position;
                    Vector3 pos = a + u * (b - a) + v * (c - a);
                    Owner.MoveTo(pos, silent: true);
                }
                else if (target is PlaneShape plane)
                {
                    var pts = plane.PivotPoints;
                    if (pts.Count < 3) return;

                    Vector3 a = pts[0].transform.position;
                    Vector3 b = pts[1].transform.position;
                    Vector3 c = pts[2].transform.position;
                    Vector3 pos = a + u * (b - a) + v * (c - a);
                    Owner.MoveTo(pos, silent: true);
                }
            }
        }

        public override void ApplyConstraint(Shape changedShape, Vector3 delta) => UpdatePosition();

        public override IEnumerable<Shape> GetRelatedShapes() => new[] { target };

        public override bool HasShape(Shape shape) => shape == target;

        public override ConstraintData Serialize() => new RelativePointConstraintData
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

        public override void Cleanup()
        {
            if (target != null)
                target.OnChanged -= OnTargetChanged;
        }
    }
}
