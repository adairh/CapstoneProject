
using UnityEngine;

namespace Manipulator
{
    public static class MidpointTool
    {
        public static void CreateMidpoint(Point a, Point b)
        {
            Vector3 pos = (a.transform.position + b.transform.position) / 2f;

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
