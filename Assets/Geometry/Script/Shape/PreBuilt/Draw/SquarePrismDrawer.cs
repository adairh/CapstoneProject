// Refactored SquarePrismDrawer with Mesh Display for base square

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePrismDrawer : IPrebuiltDrawer
    {
        private string idA, idB, idC, idD, idE, idF, idG, idH;
        private ShapeMeshDisplay meshDisplay;
        private readonly Dictionary<string, Point> points = new();
        private List<string> segmentIds;
        private readonly Dictionary<string, Segment> segments = new();

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idD = Guid.NewGuid().ToString();
            idE = Guid.NewGuid().ToString();
            idF = Guid.NewGuid().ToString();
            idG = Guid.NewGuid().ToString();
            idH = Guid.NewGuid().ToString();

            segmentIds = new List<string>();
            for (var i = 0; i < 12; i++) segmentIds.Add(Guid.NewGuid().ToString());

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
                    Id = idE, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idF, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idG, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idH, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },

                new() { Id = segmentIds[0], Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = segmentIds[1], Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = segmentIds[2], Type = "Segment", ConnectedPoints = new List<string> { idC, idD } },
                new() { Id = segmentIds[3], Type = "Segment", ConnectedPoints = new List<string> { idD, idA } },
                new() { Id = segmentIds[4], Type = "Segment", ConnectedPoints = new List<string> { idE, idF } },
                new() { Id = segmentIds[5], Type = "Segment", ConnectedPoints = new List<string> { idF, idG } },
                new() { Id = segmentIds[6], Type = "Segment", ConnectedPoints = new List<string> { idG, idH } },
                new() { Id = segmentIds[7], Type = "Segment", ConnectedPoints = new List<string> { idH, idE } },
                new() { Id = segmentIds[8], Type = "Segment", ConnectedPoints = new List<string> { idA, idE } },
                new() { Id = segmentIds[9], Type = "Segment", ConnectedPoints = new List<string> { idB, idF } },
                new() { Id = segmentIds[10], Type = "Segment", ConnectedPoints = new List<string> { idC, idG } },
                new() { Id = segmentIds[11], Type = "Segment", ConnectedPoints = new List<string> { idD, idH } }
            };

            var batch = new CreateShapeBatchAction(datas);
            batch.OnShapeSpawned = shape =>
            {
                if (shape is Point pt) points[pt.ShapeId] = pt;
                if (shape is Segment seg) segments[seg.ShapeId] = seg;
                TryConnect();
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        public void Working(Vector3 currentPos)
        {
            if (!points.ContainsKey(idA) || !points.ContainsKey(idB)) return;
            var a = points[idA];
            var b = points[idB];
            var snappedPos = currentPos;
            snappedPos.y = 0f;
            b.MoveTo(snappedPos, queue: false);
            var ab = b.transform.position - a.transform.position;
            var right = Vector3.Cross(Vector3.up, ab.normalized);
            var length = ab.magnitude;
            var height = length * 0.8f;
            points[idC].MoveTo(b.transform.position + right * length, queue: false);
            points[idD].MoveTo(a.transform.position + right * length, queue: false);
            points[idE].MoveTo(a.transform.position + Vector3.up * height, queue: false);
            points[idF].MoveTo(b.transform.position + Vector3.up * height, queue: false);
            points[idG].MoveTo(points[idC].transform.position + Vector3.up * height, queue: false);
            points[idH].MoveTo(points[idD].transform.position + Vector3.up * height, queue: false);
        }

        public void End(Vector3 finalPos)
        {
            foreach (var pt in points.Values) pt.SetRaycastIgnore(false);
            foreach (var seg in segments.Values) seg.SetRaycastIgnore(false);
        }

        public void Cancel()
        {
            foreach (var pt in points.Values) pt?.DestroyShape();
            foreach (var seg in segments.Values) seg?.DestroyShape();
        }

        private void TryConnect()
        {
            if (points.Count == 8 && segments.Count == 12)
            {
                foreach (var pt in points.Values) pt.SetRaycastIgnore(true);
                foreach (var seg in segments.Values)
                {
                    seg.MarkAsPreview();
                    seg.SetRaycastIgnore(true);
                }

                ConnectPoints(idA, idB, segmentIds[0]);
                ConnectPoints(idB, idC, segmentIds[1]);
                ConnectPoints(idC, idD, segmentIds[2]);
                ConnectPoints(idD, idA, segmentIds[3]);
                ConnectPoints(idE, idF, segmentIds[4]);
                ConnectPoints(idF, idG, segmentIds[5]);
                ConnectPoints(idG, idH, segmentIds[6]);
                ConnectPoints(idH, idE, segmentIds[7]);
                ConnectPoints(idA, idE, segmentIds[8]);
                ConnectPoints(idB, idF, segmentIds[9]);
                ConnectPoints(idC, idG, segmentIds[10]);
                ConnectPoints(idD, idH, segmentIds[11]);

                if (meshDisplay == null &&
                    points.TryGetValue(idA, out var a) &&
                    points.TryGetValue(idB, out var b) &&
                    points.TryGetValue(idC, out var c) &&
                    points.TryGetValue(idD, out var d))
                {
                    meshDisplay = a.gameObject.AddComponent<ShapeMeshDisplay>();
                    meshDisplay.Initialize(new List<Point> { a, b, c, d });
                }
            }
        }

        private void ConnectPoints(string id1, string id2, string segmentId)
        {
            if (points.TryGetValue(id1, out var p1) &&
                points.TryGetValue(id2, out var p2) &&
                segments.TryGetValue(segmentId, out var seg))
            {
                seg.SetStartPoint(p1);
                seg.SetEndPoint(p2);
            }
        }
    }
}