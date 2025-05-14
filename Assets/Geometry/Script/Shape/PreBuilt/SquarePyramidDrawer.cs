
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePyramidDrawer : IPrebuiltDrawer
    { 
        
        private Point a, b, c, d, apex;
        private List<Segment> segments = new();

        public void Begin(Vector3 startPos)
        {
            a = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            b = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            c = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            d = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            apex = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;

            foreach (var pt in new[] { a, b, c, d, apex }) pt.SetRaycastIgnore(true);

            for (int i = 0; i < 8; i++)
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
            Vector3 dir = ab.normalized;
            float side = ab.magnitude;

            Vector3 right = Vector3.Cross(dir, Vector3.forward);
            Vector3 bPos = a.transform.position + dir * side;
            Vector3 cPos = bPos + right * side;
            Vector3 dPos = a.transform.position + right * side;
            Vector3 apexPos = (a.transform.position + bPos + cPos + dPos) / 4f + Vector3.forward * (side * 0.8f);

            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            d.MoveTo(dPos, queue: false);
            apex.MoveTo(apexPos, queue: false);

            segments[0].SetStartPoint(a); segments[0].SetEndPoint(b);
            segments[1].SetStartPoint(b); segments[1].SetEndPoint(c);
            segments[2].SetStartPoint(c); segments[2].SetEndPoint(d);
            segments[3].SetStartPoint(d); segments[3].SetEndPoint(a);

            segments[4].SetStartPoint(apex); segments[4].SetEndPoint(a);
            segments[5].SetStartPoint(apex); segments[5].SetEndPoint(b);
            segments[6].SetStartPoint(apex); segments[6].SetEndPoint(c);
            segments[7].SetStartPoint(apex); segments[7].SetEndPoint(d);
        }

        public void End(Vector3 finalPos)
        {
            var batch = new CreateShapeBatchAction(new List<ShapeData>
            {
                a.Data, b.Data, c.Data, d.Data, apex.Data,
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { a.ShapeId, b.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { b.ShapeId, c.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { c.ShapeId, d.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { d.ShapeId, a.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { apex.ShapeId, a.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { apex.ShapeId, b.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { apex.ShapeId, c.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { apex.ShapeId, d.ShapeId } },
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);

            a.DestroyShape(); b.DestroyShape(); c.DestroyShape(); d.DestroyShape(); apex.DestroyShape();
            foreach (var seg in segments) seg.DestroyShape();
        }

        public void Cancel()
        {
            foreach (var pt in new[] { a, b, c, d, apex })
                pt?.DestroyShape();

            foreach (var seg in segments)
                seg.DestroyShape();
        }
    }
}
