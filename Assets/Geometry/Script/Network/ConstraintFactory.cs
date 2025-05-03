using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Manipulator
{ 

    public static class ConstraintFactory {
        private static readonly Dictionary<string, Action<ConstraintData>> ctors
            = new Dictionary<string, Action<ConstraintData>>();

        static ConstraintFactory() {
            ctors["Angle"] = data => {
                var ad    = (AngleConstraintData)data;
                var pivot = (Point)ShapeStorage.GetShapeByID(ad.PivotName);
                var a     = (Segment)ShapeStorage.GetShapeByID(ad.SegmentAName);
                var b     = (Segment)ShapeStorage.GetShapeByID(ad.SegmentBName);
                var c     = pivot.GO.AddComponent<AngleConstraint>();
                c.AddDependencies(a, b, pivot, ad.TargetAngleDeg);
            };
        }

        public static void Create(ConstraintData d) {
            if (!ctors.TryGetValue(d.Type, out var ctor))
                throw new Exception($"No ConstraintFactory for {d.Type}");
            ctor(d);
        }
        
        public static void Delete(ConstraintData cd)
    {
        if (cd == null)
        {
            Debug.LogWarning("[ConstraintFactory] ConstraintData == null");
            return;
        }

        switch (cd)
        {
            case AngleConstraintData ad:
                // 1) Lấy pivot từ ShapeStorage
                var pivotShape = ShapeStorage.GetShapeByID(ad.PivotName) as Point;
                if (pivotShape == null)
                {
                    Debug.LogWarning($"[ConstraintFactory] Pivot '{ad.PivotName}' not found");
                    return;
                }

                // 2) Lấy GameObject của pivot, nếu chưa set GO thì skip
                var pivotGO = pivotShape.GO;
                if (pivotGO == null)
                {
                    Debug.LogWarning($"[ConstraintFactory] pivotShape.GO is null for '{ad.PivotName}'");
                    return;
                }

                // 3) Lấy hết AngleConstraint gắn trên pivotGO
                var allCons = pivotGO.GetComponents<AngleConstraint>();
                if (allCons == null || allCons.Length == 0)
                {
                    Debug.Log($"[ConstraintFactory] No AngleConstraint found on '{ad.PivotName}'");
                    return;
                }

                // 4) Tìm đúng cặp segment
                foreach (var ac in allCons)
                {
                    if (ac.GetA().Name == ad.SegmentAName &&
                        ac.GetB().Name == ad.SegmentBName)
                    {
                        UnityEngine.Object.Destroy(ac);
                        Debug.Log($"[ConstraintFactory] Destroyed AngleConstraint on '{ad.PivotName}' between {ad.SegmentAName} & {ad.SegmentBName}");
                        return;
                    }
                }

                Debug.LogWarning($"[ConstraintFactory] Matching AngleConstraint not found: pivot={ad.PivotName}, segA={ad.SegmentAName}, segB={ad.SegmentBName}");
                break;

            // TODO: case cho các loại ConstraintData khác

            default:
                Debug.LogWarning($"[ConstraintFactory] Unsupported ConstraintData type: {cd.GetType().Name}");
                break;
        }
    }
        
        
    }


}