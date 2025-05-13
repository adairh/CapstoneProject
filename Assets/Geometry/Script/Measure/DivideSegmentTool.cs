
using UnityEngine;

namespace Manipulator
{
    public static class DivideSegmentTool
    {
        public static void CreateDividedPoint(Point a, Point b, float ratio)
        {
            Vector3 pa = a.transform.position;
            Vector3 pb = b.transform.position;
            Vector3 pos = (pa + ratio * pb) / (1 + ratio);

            var data = new ShapeData
            {
                Id = System.Guid.NewGuid().ToString(),
                Type = "Point",
                Position = pos,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new(),
                Settings = new()
            };

            var v = new CreateShapeAction(data);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(v);
        }
    }
}
