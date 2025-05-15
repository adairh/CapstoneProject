// Refactored SquareDrawer with Mesh Display
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquareDrawer : IPrebuiltDrawer
    {
        private string idA, idB, idC, idD;
        private string idAB, idBC, idCD, idDA;
        private Point a, b, c, d;
        private Segment ab, bc, cd, da;
        private ShapeMeshDisplay meshDisplay;

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idD = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCD = Guid.NewGuid().ToString();
            idDA = Guid.NewGuid().ToString();

            var datas = new List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idB, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idC, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idD, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idAB, Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = idBC, Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = idCD, Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = idDA, Type = "Segment", ConnectedPoints = new() { idD, idA } },
            };

            var batch = new CreateShapeBatchAction(datas);
            batch.OnShapeSpawned = shape =>
            {
                if (shape is Point pt)
                {
                    if (pt.ShapeId == idA) a = pt;
                    if (pt.ShapeId == idB) b = pt;
                    if (pt.ShapeId == idC) c = pt;
                    if (pt.ShapeId == idD) d = pt;
                }
                else if (shape is Segment s)
                {
                    if (s.ShapeId == idAB) ab = s;
                    if (s.ShapeId == idBC) bc = s;
                    if (s.ShapeId == idCD) cd = s;
                    if (s.ShapeId == idDA) da = s;
                }
                TryConnect();
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        private void TryConnect()
        {
            if (a != null && b != null && c != null && d != null &&
                ab != null && bc != null && cd != null && da != null)
            {
                foreach (var pt in new[] { a, b, c, d }) pt.SetRaycastIgnore(true);
                foreach (var seg in new[] { ab, bc, cd, da })
                {
                    seg.MarkAsPreview();
                    seg.SetRaycastIgnore(true);
                }

                ab.SetStartPoint(a); ab.SetEndPoint(b);
                bc.SetStartPoint(b); bc.SetEndPoint(c);
                cd.SetStartPoint(c); cd.SetEndPoint(d);
                da.SetStartPoint(d); da.SetEndPoint(a);

                if (meshDisplay == null)
                {
                    meshDisplay = a.gameObject.AddComponent<ShapeMeshDisplay>();
                    meshDisplay.Initialize(new List<Point> { a, b, c, d });
                }
            }
        }

        public void Working(Vector3 currentPos)
        {
            if (a == null || b == null || c == null || d == null) return;
            Vector3 snappedPos = currentPos; snappedPos.y = 0f;
            Vector3 ab = snappedPos - a.transform.position;
            Vector3 dir = ab.normalized;
            float length = ab.magnitude;
            Vector3 right = Vector3.Cross(Vector3.up, dir);
            Vector3 bPos = a.transform.position + dir * length;
            Vector3 cPos = bPos + right * length;
            Vector3 dPos = a.transform.position + right * length;
            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            d.MoveTo(dPos, queue: false);
        }

        public void End(Vector3 finalPos)
        {
            foreach (var pt in new[] { a, b, c, d }) pt.SetRaycastIgnore(false);
            foreach (var seg in new[] { ab, bc, cd, da }) seg.SetRaycastIgnore(false);
        }

        public void Cancel()
        {
            foreach (var pt in new[] { a, b, c, d }) pt?.DestroyShape();
            foreach (var seg in new[] { ab, bc, cd, da }) seg?.DestroyShape();
        }
    }
}
