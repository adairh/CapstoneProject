using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class ShapeExtrasProcessor
    {
        public static void Process(Dictionary<string, Vector3> pointMap,
            Dictionary<string, CustomPointDef> customPoints, List<string[]> extraSegments)
        {
            if (customPoints != null)
            {
                foreach (var kvp in customPoints)
                {
                    string newId = kvp.Key;
                    var def = kvp.Value;

                    try
                    {
                        switch (def.type)
                        {
                            case "midpoint":
                                if (def.from.Length == 2 && pointMap.ContainsKey(def.from[0]) &&
                                    pointMap.ContainsKey(def.from[1]))
                                    Spawn(newId, (pointMap[def.from[0]] + pointMap[def.from[1]]) / 2, newId);
                                break;

                            case "split":
                                if (def.from.Length == 2)
                                    Spawn(newId, Vector3.Lerp(pointMap[def.from[0]], pointMap[def.from[1]], def.ratio), newId);
                                break;

                            case "extend":
                                if (def.from.Length == 2)
                                {
                                    var dir = (pointMap[def.from[1]] - pointMap[def.from[0]]).normalized;
                                    Spawn(newId, pointMap[def.from[0]] + dir * def.distance, newId);
                                }
                                break;

                            case "mirror":
                                if (def.from.Length == 1)
                                {
                                    var p = pointMap[def.from[0]];
                                    if (def.axis == "x") p.x *= -1;
                                    if (def.axis == "y") p.y *= -1;
                                    if (def.axis == "z") p.z *= -1;
                                    Spawn(newId, p, newId);
                                }
                                break;

                            case "offset":
                                if (def.from.Length == 1)
                                {
                                    var p = pointMap[def.from[0]];
                                    Vector3 dir = def.axis switch
                                    {
                                        "x" => Vector3.right,
                                        "y" => Vector3.up,
                                        "z" => Vector3.forward,
                                        _ => Vector3.zero
                                    };
                                    Spawn(newId, p + dir * def.distance, newId);
                                }
                                break;

                            case "perpendicularFoot":
                                if (def.from.Length == 3 && AllExist(def.from, pointMap))
                                {
                                    var P = pointMap[def.from[0]];
                                    var A = pointMap[def.from[1]];
                                    var B = pointMap[def.from[2]];
                                    var AB = (B - A);
                                    var AP = (P - A);
                                    var t = Vector3.Dot(AP, AB.normalized);
                                    var H = A + AB.normalized * t;
                                    Spawn(newId, H, newId);
                                }
                                break;

                            case "centroid":
                                if (def.from.Length == 3 && AllExist(def.from, pointMap))
                                {
                                    var A = pointMap[def.from[0]];
                                    var B = pointMap[def.from[1]];
                                    var C = pointMap[def.from[2]];
                                    Spawn(newId, (A + B + C) / 3, newId);
                                }
                                break;

                            case "circumcenter":
                                if (def.from.Length == 3 && AllExist(def.from, pointMap))
                                {
                                    var A = pointMap[def.from[0]];
                                    var B = pointMap[def.from[1]];
                                    var C = pointMap[def.from[2]];
                                    var midAB = (A + B) / 2;
                                    var midAC = (A + C) / 2;
                                    var n1 = Vector3.Cross(B - A, Vector3.up);
                                    var n2 = Vector3.Cross(C - A, Vector3.up);
                                    Vector3 O = IntersectionPoint(midAB, midAB + n1, midAC, midAC + n2);
                                    Spawn(newId, O, newId);
                                }
                                break;

                            case "intersection":
                                if (def.from.Length == 4 && AllExist(def.from, pointMap))
                                {
                                    var A = pointMap[def.from[0]];
                                    var B = pointMap[def.from[1]];
                                    var C = pointMap[def.from[2]];
                                    var D = pointMap[def.from[3]];
                                    Spawn(newId, IntersectionPoint(A, B, C, D), newId);
                                }
                                break;

                            default:
                                Debug.LogWarning($"[Extras] Unknown point type: {def.type}");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Extras] Error processing point {newId}: {e.Message}");
                    }
                }
            }

            if (extraSegments != null)
            {
                foreach (var pair in extraSegments)
                {
                    var p1 = ShapeStorage.GetByName(pair[0]);
                    var p2 = ShapeStorage.GetByName(pair[1]);

                    if (p1 != null && p2 != null)
                    {
                        ShapeData sd = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Segment",
                            ConnectedPoints = new List<string>
                            {
                                p1.ShapeId, 
                                p2.ShapeId
                            }
                        };
                        UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeAction(sd));
                    }
                }
            }
        }

        private static void Spawn(string id, Vector3 pos, string lgcName = "")
        {
            ShapeData sd = new()
            {
                Id = id,
                LogicalName = lgcName,
                Type = "Point",
                Position = pos,
                Rotation = Quaternion.identity,
                Scale = Vector3.one
            };
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeAction(sd));
        }

        private static bool AllExist(string[] ids, Dictionary<string, Vector3> map)
        {
            foreach (var id in ids)
                if (!map.ContainsKey(id))
                    return false;
            return true;
        }

        private static Vector3 IntersectionPoint(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
        {
            Vector2 a = new Vector2(A.x, A.z);
            Vector2 b = new Vector2(B.x, B.z);
            Vector2 c = new Vector2(C.x, C.z);
            Vector2 d = new Vector2(D.x, D.z);

            Vector2 ab = b - a;
            Vector2 cd = d - c;

            float denom = ab.x * cd.y - ab.y * cd.x;
            if (Mathf.Abs(denom) < 1e-6f) return (a + b) / 2;

            float t = ((c.x - a.x) * cd.y - (c.y - a.y) * cd.x) / denom;
            Vector2 intersect2D = a + ab * t;

            return new Vector3(intersect2D.x, A.y, intersect2D.y);
        }
    }
}
