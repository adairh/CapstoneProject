using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class DivideSegmentTool
    {
        public static void CreateDividedPoint(Point a, Point b, float ratio)
        {
            var pa = a.transform.position;
            var pb = b.transform.position;
            var pos = (pa + ratio * pb) / (1 + ratio);

            var data = new ShapeData
            {
                Id = Guid.NewGuid().ToString(),
                Type = "Point",
                Position = pos,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new List<string>(),
                Settings = new Dictionary<string, string>()
            };

            var v = new CreateShapeAction(data);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(v);
        }
    }
}