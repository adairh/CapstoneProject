using System;
using System.Collections.Generic;
using UnityEngine; 

namespace Manipulator
{
    public static class ShapeFactory
    {
        private static readonly Dictionary<string, Func<Vector3, Shape>> creators = new()
        {
            {"Point", pos => Create<Point>("Point", pos)},
            {"Segment", pos => Create<Segment>("Segment", pos)}
        };

        public static Shape CreateShape(string type, Vector3 position)
        {
            if (creators.TryGetValue(type, out var ctor))
                return ctor(position);

            Debug.LogError($"[ShapeFactory] Unknown shape type: {type}");
            return null;
        }

        private static T Create<T>(string type, Vector3 position) where T : Shape
        {
            var go = new GameObject(type);
            var shape = go.AddComponent<T>();
            shape.InitializeNew(type, position);
            return shape;
        }

        public static Shape CreateFromData(ShapeData data)
        {
            var shape = CreateShape(data.Type, data.Position);
            if (shape == null)
            {
                Debug.LogError($"[ShapeFactory] Failed to create shape from data with unknown type: {data.Type}");
                return null;
            }

            if (shape != null)
            {
                shape.Deserialize(data);
            }

            return shape;
        }
    }
}