// Refactored RightTriangleDrawer with Mesh Display

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RightTriangleDrawer : IPrebuiltDrawer
    {
        private Point a, b, c;
        private Segment ab, bc, ca;
        private string idA, idB, idC, idAB, idBC, idCA;
        private ShapeMeshDisplay meshDisplay;

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCA = Guid.NewGuid().ToString();

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
                new() { Id = idAB, Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = idBC, Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = idCA, Type = "Segment", ConnectedPoints = new List<string> { idC, idA } }
            };

            var batch = new CreateShapeBatchAction(datas);
            batch.OnShapeSpawned = shape =>
            {
                if (shape is Point pt)
                {
                    if (pt.ShapeId == idA) a = pt;
                    if (pt.ShapeId == idB) b = pt;
                    if (pt.ShapeId == idC) c = pt;
                }
                else if (shape is Segment s)
                {
                    if (s.ShapeId == idAB) ab = s;
                    if (s.ShapeId == idBC) bc = s;
                    if (s.ShapeId == idCA) ca = s;
                }

                TryConnect();
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        public void Working(Vector3 currentPos)
        {
            if (a == null || b == null || c == null) return;
            var snappedPos = currentPos;
            snappedPos.y = 0f;
            b.MoveTo(snappedPos, queue: false);
            var ab = b.transform.position - a.transform.position;
            var right = Vector3.Cross(Vector3.up, ab.normalized);
            var cPos = a.transform.position + right * ab.magnitude;
            c.MoveTo(cPos, queue: false);
        }


        public void End(Vector3 finalPos)
        {
            foreach (var pt in new[] { a, b, c }) pt.SetRaycastIgnore(false);
            foreach (var seg in new[] { ab, bc, ca }) seg.SetRaycastIgnore(false);
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

        private void TryConnect()
        {
            if (a != null && b != null && c != null && ab != null && bc != null && ca != null)
            {
                foreach (var pt in new[] { a, b, c }) pt.SetRaycastIgnore(true);
                foreach (var seg in new[] { ab, bc, ca })
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

                if (meshDisplay == null)
                {
                    meshDisplay = a.gameObject.AddComponent<ShapeMeshDisplay>();
                    meshDisplay.Initialize(new List<Point> { a, b, c });
                }
            }
        }
    }
}