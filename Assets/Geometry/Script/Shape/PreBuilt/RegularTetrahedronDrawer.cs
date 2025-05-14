
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RegularTetrahedronDrawer : IPrebuiltDrawer
    {
        
         
        
        private Point a, b, c, d;
        private List<Segment> segments = new();

        public void Begin(Vector3 startPos)
        {
            a = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            b = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            c = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            d = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;

            foreach (var pt in new[] { a, b, c, d }) pt.SetRaycastIgnore(true);

            for (int i = 0; i < 6; i++)
            {
                var seg = ShapeFactory.CreateShape("Segment", startPos) as Segment;
                seg.MarkAsPreview();
                seg.SetRaycastIgnore(true);
                segments.Add(seg);
            }
        }

        public void Working(Vector3 currentPos)
        {
            Vector3 ab = currentPos - a.transform.position;
            float side = ab.magnitude;

            Vector3 bPos = a.transform.position + ab;
            Vector3 cPos = a.transform.position + Quaternion.Euler(0, 0, 60) * ab;

            // Calculate apex using tetrahedron geometry
            Vector3 centroid = (a.transform.position + bPos + cPos) / 3f;
            float height = Mathf.Sqrt(2f / 3f) * side;
            Vector3 dPos = centroid + Vector3.forward * height;

            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            d.MoveTo(dPos, queue: false);

            segments[0].SetStartPoint(a); segments[0].SetEndPoint(b);
            segments[1].SetStartPoint(b); segments[1].SetEndPoint(c);
            segments[2].SetStartPoint(c); segments[2].SetEndPoint(a);

            segments[3].SetStartPoint(a); segments[3].SetEndPoint(d);
            segments[4].SetStartPoint(b); segments[4].SetEndPoint(d);
            segments[5].SetStartPoint(c); segments[5].SetEndPoint(d);
        }

        public void End(Vector3 finalPos)
        {
            var batch = new CreateShapeBatchAction(new List<ShapeData>
            {
                a.Data, b.Data, c.Data, d.Data,
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { a.ShapeId, b.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { b.ShapeId, c.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { c.ShapeId, a.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { a.ShapeId, d.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { b.ShapeId, d.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { c.ShapeId, d.ShapeId } },
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
            foreach (var pt in new[] { a, b, c, d }) pt.DestroyShape();
            foreach (var seg in segments) seg.DestroyShape();
        }

        public void Cancel()
        {
            foreach (var pt in new[] { a, b, c, d }) pt?.DestroyShape();
            foreach (var seg in segments) seg.DestroyShape();
        }
    }
}
