using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class MidpointTool
    {
        public static void CreateMidpoint(Point a, Point b)
        {
            var pos = (a.transform.position + b.transform.position) / 2f;

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