// ShapeFactory.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    // Manipulator/Factories/ShapeFactory.cs
    using System;
    using System.Collections.Generic;
    using Manipulator.Data;
    using UnityEngine;

    public static class ShapeFactory {
        private static readonly Dictionary<string, Func<ShapeData, Shape>> ctors
            = new Dictionary<string, Func<ShapeData, Shape>>();

        static ShapeFactory() {
            ctors["Point"] = data => {
                var pd = (PointData)data;
                var p  = new Point(pd.Position);
                p.Deserialize(pd);
                return p;
            };
            ctors["Segment"] = data => {
                var sd = (SegmentData)data;
                var s  = new Segment(new Point(Vector3.zero), new Point(Vector3.zero));
                // lưu tạm rồi gắn trong Deserialize để lấy pivot từ Storage
                s.Deserialize(sd);
                return s;
            };
        }

        public static Shape Create(ShapeData d) {
            if (!ctors.TryGetValue(d.Type, out var ctor))
                throw new Exception($"No ShapeFactory for {d.Type}");
            return ctor(d);
        }
    }
 
}
