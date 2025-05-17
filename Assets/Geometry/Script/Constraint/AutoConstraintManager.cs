using System;
using UnityEngine;

namespace Manipulator
{
    public static class AutoConstraintManager
    {
        public static void TryAutoAttachConstraint(Point point)
        {
            foreach (var shape in ShapeStorage.GetAllShapes())
                if (shape is Segment seg)
                {
                    var a = seg.StartPoint.transform.position;
                    var b = seg.EndPoint.transform.position;
                    var p = point.transform.position;
                    var t = Vector3.Dot(p - a, b - a) / Vector3.Dot(b - a, b - a);
                    t = Mathf.Clamp01(t);
                    var closest = Vector3.Lerp(a, b, t);

                    if (Vector3.Distance(p, closest) < 0.1f)
                    {
                        var data = new RelativePointConstraintData
                        {
                            ConstraintId = Guid.NewGuid().ToString(),
                            PointId = point.ShapeId,
                            TargetShapeId = seg.ShapeId,
                            TargetType = RelativeTargetType.Segment,
                            T = t,
                            U = 0,
                            V = 0,
                            Type = "RelativePoint"
                        };
                        ConstraintFactory.CreateConstraintNetworked(data);
                        return;
                    }
                }
                else if (shape is Polygon poly)
                {
                    if (RelativePointHelper.FindContainingTriangleAndUV(point, poly,
                            out var ia, out var ib, out var ic, out var u, out var v))
                    {
                        var data = new RelativePointConstraintData
                        {
                            ConstraintId = Guid.NewGuid().ToString(),
                            PointId = point.ShapeId,
                            TargetShapeId = poly.ShapeId,
                            TargetType = RelativeTargetType.Plane,
                            IndexA = ia,
                            IndexB = ib,
                            IndexC = ic,
                            U = u,
                            V = v,
                            T = 0,
                            Type = "RelativePoint"
                        };
                        ConstraintFactory.CreateConstraintNetworked(data);
                        return;
                    }
                }
        }
    }
}