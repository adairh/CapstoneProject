
﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class ShapeExtrasProcessor
    {
        public static void BuildFromAI(
            Dictionary<string, CustomPointDef> customPoints,
            List<string[]> extraSegments)
        {
            var shapes = new List<ShapeData>();
            var pointMap = new Dictionary<string, Vector3>();
            var logicalToId = new Dictionary<string, string>();

            foreach (var kvp in customPoints)
            {
                string name = kvp.Key;
                CustomPointDef def = kvp.Value;
                Vector3 pos = Compute(def, pointMap);

                Point existing = FindExistingPoint(pos);
                if (existing != null)
                {
                    pointMap[name] = existing.GetCurrentPosition();
                    logicalToId[name] = existing.ShapeId;
                    continue;
                }

                string id = Guid.NewGuid().ToString();
                pointMap[name] = pos;
                logicalToId[name] = id;

                shapes.Add(new ShapeData
                {
                    Id = id,
                    Type = "Point",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    LogicalName = name,
                    ConnectedPoints = new List<string>(),
                    Settings = new Dictionary<string, string>()
                });
            }

            foreach (var segment in extraSegments)
            {
                if (segment.Length != 2 ||
                    !logicalToId.ContainsKey(segment[0]) ||
                    !logicalToId.ContainsKey(segment[1]))
                    continue;

                shapes.Add(new ShapeData
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Segment",
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string>
                    {
                        logicalToId[segment[0]],
                        logicalToId[segment[1]]
                    },
                    Settings = new Dictionary<string, string>()
                });
            }

            var batch = new CreateShapeBatchAction(shapes);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        private static Vector3 Compute(CustomPointDef def, Dictionary<string, Vector3> pointMap)
        {
            Vector3 p1, p2, p3;
            switch (def.type)
            {
                case "absolute":
                    if (def.position != null && def.position.Length == 3)
                        return new Vector3(def.position[0], def.position[1], def.position[2]);
                    break;

                case "midpoint":
                    if (def.from?.Length == 2 &&
                        pointMap.TryGetValue(def.from[0], out p1) &&
                        pointMap.TryGetValue(def.from[1], out p2))
                        return (p1 + p2) / 2;
                    break;

                case "split":
                case "on_segment":
                    if (def.from?.Length == 2 &&
                        pointMap.TryGetValue(def.from[0], out p1) &&
                        pointMap.TryGetValue(def.from[1], out p2))
                        return Vector3.Lerp(p1, p2, def.ratio);
                    break;

                case "centroid":
                    if (def.from?.Length == 3 &&
                        pointMap.TryGetValue(def.from[0], out p1) &&
                        pointMap.TryGetValue(def.from[1], out p2) &&
                        pointMap.TryGetValue(def.from[2], out p3))
                        return (p1 + p2 + p3) / 3;
                    break;

                case "equidistant":
                    Vector3 sum = Vector3.zero;
                    int count = 0;
                    foreach (var key in def.from)
                    {
                        if (pointMap.TryGetValue(key, out var pt))
                        {
                            sum += pt;
                            count++;
                        }
                    }
                    return count > 0 ? sum / count : Vector3.zero;

                case "extend":
                    if (def.from?.Length == 2 &&
                        pointMap.TryGetValue(def.from[0], out p1) &&
                        pointMap.TryGetValue(def.from[1], out p2))
                    {
                        Vector3 dir = (p2 - p1).normalized;
                        return p1 + dir * def.distance;
                    }
                    break;

                case "perpendicularFoot":
                    if (def.from?.Length == 3 &&
                        pointMap.TryGetValue(def.from[0], out var P) &&
                        pointMap.TryGetValue(def.from[1], out var A) &&
                        pointMap.TryGetValue(def.from[2], out var B))
                    {
                        var AB = B - A;
                        var AP = P - A;
                        float t = Vector3.Dot(AP, AB.normalized);
                        return A + AB.normalized * t;
                    }
                    break;

                case "arbitrary":
                    if (def.from?.Length == 2 &&
                        pointMap.TryGetValue(def.from[0], out p1) &&
                        pointMap.TryGetValue(def.from[1], out p2))
                        return Vector3.Lerp(p1, p2, 0.3f);
                    break;
            }

            return Vector3.zero;
        }

        private static Point FindExistingPoint(Vector3 pos)
        {
            const float threshold = 0.01f;
            foreach (var shape in ShapeStorage.GetAllShapes())
            {
                if (shape is Point pt && Vector3.Distance(pt.transform.position, pos) < threshold)
                    return pt;
            }
            return null;
        }
    }
}
