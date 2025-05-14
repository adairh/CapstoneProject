
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralPyramidDrawer : IPrebuiltDrawer
    {
        
        public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.EquilateralPyramid;
 
        
        private Point a, b, c, apex;
        private List<Segment> segments = new();

        public void Begin(Vector3 startPos)
        {
            a = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            b = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            c = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;
            apex = ShapeFactory.CreateShape(Guid.NewGuid().ToString(), startPos) as Point;

            foreach (var pt in new[] { a, b, c, apex }) pt.SetRaycastIgnore(true);

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
            Vector3 apexPos = (a.transform.position + bPos + cPos) / 3f + Vector3.forward * side;

            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            apex.MoveTo(apexPos, queue: false);

            segments[0].SetStartPoint(a); segments[0].SetEndPoint(b);
            segments[1].SetStartPoint(b); segments[1].SetEndPoint(c);
            segments[2].SetStartPoint(c); segments[2].SetEndPoint(a);

            segments[3].SetStartPoint(apex); segments[3].SetEndPoint(a);
            segments[4].SetStartPoint(apex); segments[4].SetEndPoint(b);
            segments[5].SetStartPoint(apex); segments[5].SetEndPoint(c);
        }

        public void End(Vector3 finalPos)
        {
            var batch = new CreateShapeBatchAction(new List<ShapeData>
            {
                a.Data, b.Data, c.Data, apex.Data,
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { a.ShapeId, b.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { b.ShapeId, c.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { c.ShapeId, a.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { apex.ShapeId, a.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { apex.ShapeId, b.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { apex.ShapeId, c.ShapeId } },
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);

            a.DestroyShape(); b.DestroyShape(); c.DestroyShape(); apex.DestroyShape();
            foreach (var seg in segments) seg.DestroyShape();
        }

        public void Cancel()
        {
            a?.DestroyShape(); b?.DestroyShape(); c?.DestroyShape(); apex?.DestroyShape();
            foreach (var seg in segments) seg.DestroyShape();
        }
    }
}
