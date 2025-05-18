using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    /// <summary>
    ///     Lưu toàn bộ các shape hiện tại trong scene, tra cứu theo ID hoặc Name.
    /// </summary>
    public static class ShapeStorage
    {
        private static readonly Dictionary<string, Shape> idLookup = new();
        private static readonly Dictionary<string, Shape> nameLookup = new();

        public static void Register(Shape shape)
        {
            if (string.IsNullOrEmpty(shape.ShapeId)) return;

            if (idLookup.ContainsKey(shape.ShapeId))
                Debug.LogWarning($"Duplicate ShapeId detected: {shape.ShapeId}");

            idLookup[shape.ShapeId] = shape;
            nameLookup[shape.name] = shape;
        }

        public static void Unregister(Shape shape)
        {
            idLookup.Remove(shape.ShapeId);
            nameLookup.Remove(shape.name);
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
        }
    }
}