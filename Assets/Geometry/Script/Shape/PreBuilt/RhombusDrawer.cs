
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RhombusDrawer : IPrebuiltDrawer
    {
        
         
        
        private Point a, b, c, d;
        private List<Segment> segments = new();

        public void Begin(Vector3 startPos)
        {
            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();

            a = ShapeFactory.CreateShape(idA, startPos) as Point;
            b = ShapeFactory.CreateShape(idB, startPos) as Point;
            c = ShapeFactory.CreateShape(idC, startPos) as Point;
            d = ShapeFactory.CreateShape(idD, startPos) as Point;

            foreach (var pt in new[] { a, b, c, d }) pt.SetRaycastIgnore(true);

            for (int i = 0; i < 4; i++)
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
            float length = ab.magnitude;

            Vector3 right = Vector3.Cross(dir, Vector3.forward);
            float diagonal = length;

            Vector3 bPos = a.transform.position + dir * length;
            Vector3 cPos = bPos + right * diagonal;
            Vector3 dPos = a.transform.position + right * diagonal;

            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            d.MoveTo(dPos, queue: false);

            segments[0].SetStartPoint(a); segments[0].SetEndPoint(b);
            segments[1].SetStartPoint(b); segments[1].SetEndPoint(c);
            segments[2].SetStartPoint(c); segments[2].SetEndPoint(d);
            segments[3].SetStartPoint(d); segments[3].SetEndPoint(a);
        }

        public void End(Vector3 finalPos)
        {
            var batch = new CreateShapeBatchAction(new List<ShapeData>
            {
                a.Data, b.Data, c.Data, d.Data,
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { a.ShapeId, b.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { b.ShapeId, c.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { c.ShapeId, d.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { d.ShapeId, a.ShapeId } }
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);

            a.DestroyShape(); b.DestroyShape(); c.DestroyShape(); d.DestroyShape();
            foreach (var seg in segments) seg.DestroyShape();
        }

        public void Cancel()
        {
            a?.DestroyShape(); b?.DestroyShape(); c?.DestroyShape(); d?.DestroyShape();
            foreach (var seg in segments) seg.DestroyShape();
        }
    }
}
