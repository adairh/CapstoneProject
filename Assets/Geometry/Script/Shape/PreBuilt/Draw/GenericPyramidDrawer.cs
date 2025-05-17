// Refactored GenericPyramidDrawer

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class GenericPyramidDrawer : IPrebuiltDrawer
    {
        private Point a, b, c, apex;
        private Segment ab, bc, ca, ad, bd, cd;
        private string idA, idB, idC, idD;
        private string idAB, idBC, idCA, idAD, idBD, idCD;

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
                new()
                {
                    Id = idA, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idB, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idC, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idD, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },

                new() { Id = idAB, Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = idBC, Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = idCA, Type = "Segment", ConnectedPoints = new List<string> { idC, idA } },
                new() { Id = idAD, Type = "Segment", ConnectedPoints = new List<string> { idD, idA } },
                new() { Id = idBD, Type = "Segment", ConnectedPoints = new List<string> { idD, idB } },
                new() { Id = idCD, Type = "Segment", ConnectedPoints = new List<string> { idD, idC } }
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

        public void Working(Vector3 currentPos)
        {
            if (a == null || b == null || c == null || apex == null) return;

            var snappedPos = currentPos;
            snappedPos.y = 0f;
            var ab = snappedPos - a.transform.position;
            var side = ab.magnitude;
            var dir = ab.normalized;

            var bPos = a.transform.position + dir * side;
            var cPos = a.transform.position + Quaternion.AngleAxis(-45f, Vector3.up) * dir * side * 0.8f;
            var apexPos = (a.transform.position + bPos + cPos) / 3f + Vector3.up * (side * 0.8f);

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

                ab.SetStartPoint(a);
                ab.SetEndPoint(b);
                bc.SetStartPoint(b);
                bc.SetEndPoint(c);
                ca.SetStartPoint(c);
                ca.SetEndPoint(a);
                ad.SetStartPoint(apex);
                ad.SetEndPoint(a);
                bd.SetStartPoint(apex);
                bd.SetEndPoint(b);
                cd.SetStartPoint(apex);
                cd.SetEndPoint(c);
            }
        }
    }
}