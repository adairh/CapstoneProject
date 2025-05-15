// Refactored SquarePrismDrawer
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePrismDrawer : IPrebuiltDrawer
    {
        private string idA, idB, idC, idD, idE, idF, idG, idH;
        private List<string> segmentIds;
        private Dictionary<string, Point> points = new();
        private Dictionary<string, Segment> segments = new();

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

            segmentIds = new();
            for (int i = 0; i < 12; i++) segmentIds.Add(Guid.NewGuid().ToString());

            var datas = new List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idB, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idC, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idD, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idE, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idF, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idG, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
                new ShapeData { Id = idH, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },

                new ShapeData { Id = segmentIds[0], Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = segmentIds[1], Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = segmentIds[2], Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = segmentIds[3], Type = "Segment", ConnectedPoints = new() { idD, idA } },
                new ShapeData { Id = segmentIds[4], Type = "Segment", ConnectedPoints = new() { idE, idF } },
                new ShapeData { Id = segmentIds[5], Type = "Segment", ConnectedPoints = new() { idF, idG } },
                new ShapeData { Id = segmentIds[6], Type = "Segment", ConnectedPoints = new() { idG, idH } },
                new ShapeData { Id = segmentIds[7], Type = "Segment", ConnectedPoints = new() { idH, idE } },
                new ShapeData { Id = segmentIds[8], Type = "Segment", ConnectedPoints = new() { idA, idE } },
                new ShapeData { Id = segmentIds[9], Type = "Segment", ConnectedPoints = new() { idB, idF } },
                new ShapeData { Id = segmentIds[10], Type = "Segment", ConnectedPoints = new() { idC, idG } },
                new ShapeData { Id = segmentIds[11], Type = "Segment", ConnectedPoints = new() { idD, idH } },
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

        public void Working(Vector3 currentPos)
        {
            if (!points.ContainsKey(idA) || !points.ContainsKey(idB)) return;

            var a = points[idA];
            var b = points[idB];
            b.MoveTo(currentPos, queue: false);

            Vector3 ab = b.transform.position - a.transform.position;
            Vector3 right = Vector3.Cross(ab.normalized, Vector3.forward);
            float length = ab.magnitude;
            float height = length * 0.8f;

            points[idC].MoveTo(b.transform.position + right * length, queue: false);
            points[idD].MoveTo(a.transform.position + right * length, queue: false);
            points[idE].MoveTo(a.transform.position + Vector3.forward * height, queue: false);
            points[idF].MoveTo(b.transform.position + Vector3.forward * height, queue: false);
            points[idG].MoveTo(points[idC].transform.position + Vector3.forward * height, queue: false);
            points[idH].MoveTo(points[idD].transform.position + Vector3.forward * height, queue: false);
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
    }
}
