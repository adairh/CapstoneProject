using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class ShapeStorage
    {
        private static readonly Dictionary<string, Shape> idLookup = new();
        private static readonly Dictionary<string, Shape> nameLookup = new();
        private static readonly List<Shape> allShapes = new();

        public static void Register(Shape shape)
        {
            if (string.IsNullOrEmpty(shape.ShapeId)) return;

            if (idLookup.ContainsKey(shape.ShapeId))
                Debug.LogWarning($"Duplicate ShapeId detected: {shape.ShapeId}");

            idLookup[shape.ShapeId] = shape;
            nameLookup[shape.name] = shape;
            allShapes.Add(shape);
        }

        public static void Unregister(Shape shape)
        {
            idLookup.Remove(shape.ShapeId);
            nameLookup.Remove(shape.name);
            allShapes.Remove(shape);
        }

        public static bool Contains(string id)
        {
            return idLookup.ContainsKey(id);
        }

        public static Shape GetById(string id)
        {
            return idLookup.TryGetValue(id, out var s) ? s : null;
        }

        public static Shape GetByName(string name)
        {
            return nameLookup.TryGetValue(name, out var s) ? s : null;
        }

        public static IEnumerable<Shape> GetAllShapes()
        {
            return idLookup.Values;
        }

        public static void Clear()
        {
            idLookup.Clear();
            nameLookup.Clear();
            allShapes.Clear();
        }
        
        public static Segment GetMostRecentSegment()
        {
            for (int i = allShapes.Count - 1; i >= 0; i--)
            {
                if (allShapes[i] is Segment seg)
                    return seg;
            }
            return null;
        }
        public static Point GetMostRecentPoint()
        {
            for (int i = allShapes.Count - 1; i >= 0; i--)
            {
                if (allShapes[i] is Point seg)
                    return seg;
            }
            return null;
        }
    }
}