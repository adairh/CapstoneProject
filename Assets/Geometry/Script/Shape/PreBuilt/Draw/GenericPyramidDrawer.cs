// Refactored GenericPyramidDrawer
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class GenericPyramidDrawer : IPrebuiltDrawer
    {
        private string idA, idB, idC, idD;
        private string idAB, idBC, idCA, idAD, idBD, idCD;
        private Point a, b, c, apex;
        private Segment ab, bc, ca, ad, bd, cd;

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idD = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCA = Guid.NewGuid().ToString();
            idAD = Guid.NewGuid().ToString();
            idBD = Guid.NewGuid().ToString();
            idCD = Guid.NewGuid().ToString();

            var datas = new List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idB, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idC, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idD, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },

                new ShapeData { Id = idAB, Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = idBC, Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = idCA, Type = "Segment", ConnectedPoints = new() { idC, idA } },
                new ShapeData { Id = idAD, Type = "Segment", ConnectedPoints = new() { idD, idA } },
                new ShapeData { Id = idBD, Type = "Segment", ConnectedPoints = new() { idD, idB } },
                new ShapeData { Id = idCD, Type = "Segment", ConnectedPoints = new() { idD, idC } },
            };

            var batch = new CreateShapeBatchAction(datas);
            batch.OnShapeSpawned = shape =>
            {
                if (shape is Point pt)
                {
                    if (pt.ShapeId == idA) a = pt;
                    if (pt.ShapeId == idB) b = pt;
                    if (pt.ShapeId == idC) c = pt;
                    if (pt.ShapeId == idD) apex = pt;
                }
                else if (shape is Segment s)
                {
                    if (s.ShapeId == idAB) ab = s;
                    if (s.ShapeId == idBC) bc = s;
                    if (s.ShapeId == idCA) ca = s;
                    if (s.ShapeId == idAD) ad = s;
                    if (s.ShapeId == idBD) bd = s;
                    if (s.ShapeId == idCD) cd = s;
                }
                TryConnect();
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        private void TryConnect()
        {
            if (a != null && b != null && c != null && apex != null &&
                ab != null && bc != null && ca != null &&
                ad != null && bd != null && cd != null)
            {
                foreach (var pt in new[] { a, b, c, apex }) pt.SetRaycastIgnore(true);
                foreach (var seg in new[] { ab, bc, ca, ad, bd, cd })
                {
                    seg.MarkAsPreview();
                    seg.SetRaycastIgnore(true);
                }

                ab.SetStartPoint(a); ab.SetEndPoint(b);
                bc.SetStartPoint(b); bc.SetEndPoint(c);
                ca.SetStartPoint(c); ca.SetEndPoint(a);
                ad.SetStartPoint(apex); ad.SetEndPoint(a);
                bd.SetStartPoint(apex); bd.SetEndPoint(b);
                cd.SetStartPoint(apex); cd.SetEndPoint(c);
            }
        }

        public void Working(Vector3 currentPos)
        {
            if (a == null || b == null || c == null || apex == null) return;

            Vector3 snappedPos = currentPos; snappedPos.y = 0f;
            Vector3 ab = snappedPos - a.transform.position;
            float side = ab.magnitude;
            Vector3 dir = ab.normalized;

            Vector3 bPos = a.transform.position + dir * side;
            Vector3 cPos = a.transform.position + Quaternion.AngleAxis(-45f, Vector3.up) * dir * side * 0.8f;
            Vector3 apexPos = (a.transform.position + bPos + cPos) / 3f + Vector3.up * (side * 0.8f);

            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            apex.MoveTo(apexPos, queue: false);
        }


        public void End(Vector3 finalPos)
        {
            foreach (var pt in new[] { a, b, c, apex }) pt.SetRaycastIgnore(false);
            foreach (var seg in new[] { ab, bc, ca, ad, bd, cd }) seg.SetRaycastIgnore(false);
        }

        public void Cancel()
        {
            foreach (var pt in new[] { a, b, c, apex }) pt?.DestroyShape();
            foreach (var seg in new[] { ab, bc, ca, ad, bd, cd }) seg?.DestroyShape();
        }
    }
}
