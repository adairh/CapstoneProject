// Refactored SquarePyramidDrawer with Mesh Display for base square

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePyramidDrawer : IPrebuiltDrawer
    {
        private Point a, b, c, d, apex;
        private Segment ab, bc, cd, da, aa, ba, ca, da2;
        private string idA, idB, idC, idD, idApex;
        private string idAB, idBC, idCD, idDA, idAApex, idBApex, idCApex, idDApex;
        private ShapeMeshDisplay meshDisplay;

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idD = Guid.NewGuid().ToString();
            idApex = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCD = Guid.NewGuid().ToString();
            idDA = Guid.NewGuid().ToString();
            idAApex = Guid.NewGuid().ToString();
            idBApex = Guid.NewGuid().ToString();
            idCApex = Guid.NewGuid().ToString();
            idDApex = Guid.NewGuid().ToString();

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
                new()
                {
                    Id = idApex, Type = "Point", Position = startPos, Rotation = Quaternion.identity,
                    Scale = Vector3.one
                },

                new() { Id = idAB, Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = idBC, Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = idCD, Type = "Segment", ConnectedPoints = new List<string> { idC, idD } },
                new() { Id = idDA, Type = "Segment", ConnectedPoints = new List<string> { idD, idA } },

                new() { Id = idAApex, Type = "Segment", ConnectedPoints = new List<string> { idA, idApex } },
                new() { Id = idBApex, Type = "Segment", ConnectedPoints = new List<string> { idB, idApex } },
                new() { Id = idCApex, Type = "Segment", ConnectedPoints = new List<string> { idC, idApex } },
                new() { Id = idDApex, Type = "Segment", ConnectedPoints = new List<string> { idD, idApex } }
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
                    if (pt.ShapeId == idApex) apex = pt;
                }
                else if (shape is Segment s)
                {
                    if (s.ShapeId == idAB) ab = s;
                    if (s.ShapeId == idBC) bc = s;
                    if (s.ShapeId == idCD) cd = s;
                    if (s.ShapeId == idDA) da = s;
                    if (s.ShapeId == idAApex) aa = s;
                    if (s.ShapeId == idBApex) ba = s;
                    if (s.ShapeId == idCApex) ca = s;
                    if (s.ShapeId == idDApex) da2 = s;
                }

                TryConnect();
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        public void Working(Vector3 currentPos)
        {
            if (a == null || b == null || c == null || d == null || apex == null) return;
            var snappedPos = currentPos;
            snappedPos.y = 0f;
            var ab = snappedPos - a.transform.position;
            var dir = ab.normalized;
            var side = ab.magnitude;
            var right = Vector3.Cross(Vector3.up, dir);
            var bPos = a.transform.position + dir * side;
            var cPos = bPos + right * side;
            var dPos = a.transform.position + right * side;
            var apexPos = (a.transform.position + bPos + cPos + dPos) / 4f + Vector3.up * (side * 0.8f);
            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            d.MoveTo(dPos, queue: false);
            apex.MoveTo(apexPos, queue: false);
        }

        public void End(Vector3 finalPos)
        {
            foreach (var pt in new[] { a, b, c, d, apex }) pt.SetRaycastIgnore(false);
            foreach (var seg in new[] { ab, bc, cd, da, aa, ba, ca, da2 }) seg.SetRaycastIgnore(false);
        }

        public void Cancel()
        {
            foreach (var pt in new[] { a, b, c, d, apex }) pt?.DestroyShape();
            foreach (var seg in new[] { ab, bc, cd, da, aa, ba, ca, da2 }) seg?.DestroyShape();
        }

        private void TryConnect()
        {
            if (a != null && b != null && c != null && d != null && apex != null &&
                ab != null && bc != null && cd != null && da != null &&
                aa != null && ba != null && ca != null && da2 != null)
            {
                foreach (var pt in new[] { a, b, c, d, apex }) pt.SetRaycastIgnore(true);
                foreach (var seg in new[] { ab, bc, cd, da, aa, ba, ca, da2 })
                {
                    seg.MarkAsPreview();
                    seg.SetRaycastIgnore(true);
                }

                ab.SetStartPoint(a);
                ab.SetEndPoint(b);
                bc.SetStartPoint(b);
                bc.SetEndPoint(c);
                cd.SetStartPoint(c);
                cd.SetEndPoint(d);
                da.SetStartPoint(d);
                da.SetEndPoint(a);
                aa.SetStartPoint(a);
                aa.SetEndPoint(apex);
                ba.SetStartPoint(b);
                ba.SetEndPoint(apex);
                ca.SetStartPoint(c);
                ca.SetEndPoint(apex);
                da2.SetStartPoint(d);
                da2.SetEndPoint(apex);

                if (meshDisplay == null)
                {
                    meshDisplay = a.gameObject.AddComponent<ShapeMeshDisplay>();
                    meshDisplay.Initialize(new List<Point> { a, b, c, d });
                }
            }
        }
    }
}