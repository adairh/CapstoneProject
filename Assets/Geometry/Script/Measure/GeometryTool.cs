
using UnityEngine;

namespace Manipulator
{
    public static class GeometryTool
    {
        public static Segment CreateParallelThrough(Point a, Point b, Point p, float length = 2f)
        {
            Vector3 dir = (b.transform.position - a.transform.position).normalized;
            return CreateSegmentFromDirection(p.transform.position, dir, length);
        }

        public static Segment CreatePerpendicularThrough(Point a, Point b, Point p, float length = 2f)
        {
            Vector3 dir = (b.transform.position - a.transform.position).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
            if (perp == Vector3.zero) perp = Vector3.Cross(dir, Vector3.forward).normalized;

            return CreateSegmentFromDirection(p.transform.position, perp, length);
        }

        private static Segment CreateSegmentFromDirection(Vector3 center, Vector3 dir, float length)
        {
            Vector3 p1 = center - dir * (length / 2f);
            Vector3 p2 = center + dir * (length / 2f);

            string id1 = System.Guid.NewGuid().ToString();
            string id2 = System.Guid.NewGuid().ToString();
            string idSeg = System.Guid.NewGuid().ToString();

            var point1 = new ShapeData
            {
                Id = id1,
                Type = "Point",
                Position = p1,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new(),
                Settings = new()
            };

            var point2 = new ShapeData
            {
                Id = id2,
                Type = "Point",
                Position = p2,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new(),
                Settings = new()
            };

            var segment = new ShapeData
            {
                Id = idSeg,
                Type = "Segment",
                Position = center,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new() { id1, id2 },
                Settings = new()
            };

            var batch = new CreateShapeBatchAction(new() { point1, point2, segment });
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);

            return null; // do callback batch xử lý
        }
    }
}
