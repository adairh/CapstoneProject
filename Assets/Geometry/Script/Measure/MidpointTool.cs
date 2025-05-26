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

            var createAction = new CreateShapeAction(data);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(createAction);

            // Now find the newly created point
            // (You may want to return the new point's ID from CreateShapeAction or look it up by position/ID.)
            // Assuming ShapeStorage.GetMostRecentPoint() is available:
            var midpoint = ShapeStorage.GetMostRecentPoint();
            if (midpoint == null) return;

            // Find the segment between a and b
            // (If you used the segment directly, pass it; otherwise, find the segment between a and b)
            Segment seg = FindSegmentBetween(a, b);
            if (seg == null) return;

            // Attach relative constraint
            var constraint = midpoint.gameObject.AddComponent<RelativePointConstraint>();
            constraint.Owner = midpoint;
            constraint.SetTarget(seg, RelativeTargetType.Segment, 0.5f, 0, 0);
        }
        
        public static Segment FindSegmentBetween(Point a, Point b)
        {
            foreach (var shape in ShapeStorage.GetAllShapes())
            {
                if (shape is Segment seg)
                {
                    if ((seg.StartPoint == a && seg.EndPoint == b) || (seg.StartPoint == b && seg.EndPoint == a))
                        return seg;
                }
            }
            return null;
        }


    }
}