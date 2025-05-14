
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class IsoscelesTriangleDrawer : IPrebuiltDrawer
    {
         
        private Point a, b, c;
        private Segment ab, bc, ca;

        public void Begin(Vector3 startPos)
        {
            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();

            a = ShapeFactory.CreateShape(idA, startPos) as Point;
            b = ShapeFactory.CreateShape(idB, startPos) as Point;
            c = ShapeFactory.CreateShape(idC, startPos) as Point;

            a.SetRaycastIgnore(true);
            b.SetRaycastIgnore(true);
            c.SetRaycastIgnore(true);

            ab = ShapeFactory.CreateShape("Segment", startPos) as Segment;
            bc = ShapeFactory.CreateShape("Segment", startPos) as Segment;
            ca = ShapeFactory.CreateShape("Segment", startPos) as Segment;

            ab.MarkAsPreview();
            bc.MarkAsPreview();
            ca.MarkAsPreview();

            ab.SetStartPoint(a);
            ab.SetEndPoint(b);
            bc.SetStartPoint(b);
            bc.SetEndPoint(c);
            ca.SetStartPoint(c);
            ca.SetEndPoint(a);
        }

        public void Working(Vector3 currentPos)
        {
            b.MoveTo(currentPos, queue: false);

            Vector3 mid = (a.transform.position + b.transform.position) / 2f;
            Vector3 dir = (b.transform.position - a.transform.position).normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.forward); // mặt phẳng XY

            float baseLength = Vector3.Distance(a.transform.position, b.transform.position);
            float height = baseLength * 0.75f;
            Vector3 cPos = mid + normal * height;

            c.MoveTo(cPos, queue: false);
        }

        public void End(Vector3 finalPos)
        {
            var batch = new CreateShapeBatchAction(new List<ShapeData>
            {
                a.Data, b.Data, c.Data,
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { a.ShapeId, b.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { b.ShapeId, c.ShapeId } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { c.ShapeId, a.ShapeId } }
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);

            a.DestroyShape(); b.DestroyShape(); c.DestroyShape();
            ab.DestroyShape(); bc.DestroyShape(); ca.DestroyShape();
        }

        public void Cancel()
        {
            a?.DestroyShape();
            b?.DestroyShape();
            c?.DestroyShape();
            ab?.DestroyShape();
            bc?.DestroyShape();
            ca?.DestroyShape();
        }
    }
}
