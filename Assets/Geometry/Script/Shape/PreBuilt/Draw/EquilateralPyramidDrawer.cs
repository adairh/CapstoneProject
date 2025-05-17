// Refactored EquilateralPyramidDrawer with Mesh Display

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralPyramidDrawer : IPrebuiltDrawer
    {
        private Point a, b, c, apex;
        private ShapeMeshDisplay baseMesh;
        private string idA, idB, idC, idApex;
        private List<string> segIds;
        private readonly List<Segment> segments = new();

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idApex = Guid.NewGuid().ToString();
            segIds = new List<string>();
            for (var i = 0; i < 6; i++) segIds.Add(Guid.NewGuid().ToString());

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
                    Id = idApex, Type = "Point", Position = startPos, Rotation = Quaternion.identity,
                    Scale = Vector3.one
                },

                new() { Id = segIds[0], Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = segIds[1], Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = segIds[2], Type = "Segment", ConnectedPoints = new List<string> { idC, idA } },
                new() { Id = segIds[3], Type = "Segment", ConnectedPoints = new List<string> { idApex, idA } },
                new() { Id = segIds[4], Type = "Segment", ConnectedPoints = new List<string> { idApex, idB } },
                new() { Id = segIds[5], Type = "Segment", ConnectedPoints = new List<string> { idApex, idC } }
            };

            var batch = new CreateShapeBatchAction(datas);
            batch.OnShapeSpawned = shape =>
            {
                if (shape is Point pt)
                {
                    if (pt.ShapeId == idA) a = pt;
                    if (pt.ShapeId == idB) b = pt;
                    if (pt.ShapeId == idC) c = pt;
                    if (pt.ShapeId == idApex) apex = pt;
                }
                else if (shape is Segment seg)
                {
                    segments.Add(seg);
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
            var bPos = a.transform.position + ab;
            var cPos = a.transform.position + Quaternion.AngleAxis(60, Vector3.up) * ab;
            var apexPos = (a.transform.position + bPos + cPos) / 3f + Vector3.up * side;
            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            apex.MoveTo(apexPos, queue: false);
        }


        public void End(Vector3 finalPos)
        {
            foreach (var pt in new[] { a, b, c, apex }) pt.SetRaycastIgnore(false);
            foreach (var seg in segments) seg.SetRaycastIgnore(false);
        }

        public void Cancel()
        {
            a?.DestroyShape();
            b?.DestroyShape();
            c?.DestroyShape();
            apex?.DestroyShape();
            foreach (var seg in segments) seg?.DestroyShape();
        }

        private void TryConnect()
        {
            if (a == null || b == null || c == null || apex == null) return;
            if (segments.Count < 6) return;

            foreach (var pt in new[] { a, b, c, apex }) pt.SetRaycastIgnore(true);
            foreach (var seg in segments)
            {
                seg.MarkAsPreview();
                seg.SetRaycastIgnore(true);
            }

            segments[0].SetStartPoint(a);
            segments[0].SetEndPoint(b);
            segments[1].SetStartPoint(b);
            segments[1].SetEndPoint(c);
            segments[2].SetStartPoint(c);
            segments[2].SetEndPoint(a);
            segments[3].SetStartPoint(apex);
            segments[3].SetEndPoint(a);
            segments[4].SetStartPoint(apex);
            segments[4].SetEndPoint(b);
            segments[5].SetStartPoint(apex);
            segments[5].SetEndPoint(c);

            if (baseMesh == null)
            {
                baseMesh = a.gameObject.AddComponent<ShapeMeshDisplay>();
                baseMesh.Initialize(new List<Point> { a, b, c });
            }
        }
    }
}