
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePrismDrawer : IPrebuiltDrawer
    {
        
         
        
        private Point a, b, c, d, a2, b2, c2, d2;
        private List<Segment> segments = new();

        public void Begin(Vector3 startPos)
        {
            a = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            b = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            c = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            d = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;

            a2 = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            b2 = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            c2 = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            d2 = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;

            foreach (var pt in new[] { a, b, c, d, a2, b2, c2, d2 }) pt.SetRaycastIgnore(true);

            for (int i = 0; i < 12; i++)
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
            Vector3 dir = ab.normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.forward);
            float height = side;

            Vector3 bPos = a.transform.position + dir * side;
            Vector3 cPos = bPos + right * side;
            Vector3 dPos = a.transform.position + right * side;

            Vector3 a2Pos = a.transform.position + Vector3.forward * height;
            Vector3 b2Pos = bPos + Vector3.forward * height;
            Vector3 c2Pos = cPos + Vector3.forward * height;
            Vector3 d2Pos = dPos + Vector3.forward * height;

            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            d.MoveTo(dPos, queue: false);
            a2.MoveTo(a2Pos, queue: false);
            b2.MoveTo(b2Pos, queue: false);
            c2.MoveTo(c2Pos, queue: false);
            d2.MoveTo(d2Pos, queue: false);

            a.MoveTo(a.transform.position, queue: false); // fix glitch draw

            segments[0].SetStartPoint(a); segments[0].SetEndPoint(b);
            segments[1].SetStartPoint(b); segments[1].SetEndPoint(c);
            segments[2].SetStartPoint(c); segments[2].SetEndPoint(d);
            segments[3].SetStartPoint(d); segments[3].SetEndPoint(a);

            segments[4].SetStartPoint(a2); segments[4].SetEndPoint(b2);
            segments[5].SetStartPoint(b2); segments[5].SetEndPoint(c2);
            segments[6].SetStartPoint(c2); segments[6].SetEndPoint(d2);
            segments[7].SetStartPoint(d2); segments[7].SetEndPoint(a2);

            segments[8].SetStartPoint(a); segments[8].SetEndPoint(a2);
            segments[9].SetStartPoint(b); segments[9].SetEndPoint(b2);
            segments[10].SetStartPoint(c); segments[10].SetEndPoint(c2);
            segments[11].SetStartPoint(d); segments[11].SetEndPoint(d2);
        }

        public void End(Vector3 finalPos)
        {
            var batch = new CreateShapeBatchAction(new List<ShapeData>
            {
                a.Data, b.Data, c.Data, d.Data, a2.Data, b2.Data, c2.Data, d2.Data
            });

            foreach (var seg in segments)
            {
                var s = new ShapeData
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Segment",
                    ConnectedPoints = new List<string> { seg.StartPoint.ShapeId, seg.EndPoint.ShapeId }
                };
                batch.shapeDataList.Add(s);
            }

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
            foreach (var pt in new[] { a, b, c, d, a2, b2, c2, d2 }) pt.DestroyShape();
            foreach (var s in segments) s.DestroyShape();
        }

        public void Cancel()
        {
            foreach (var pt in new[] { a, b, c, d, a2, b2, c2, d2 }) pt?.DestroyShape();
            foreach (var s in segments) s.DestroyShape();
        }
    }
}
