using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class PointMapHelper
    {
        public static Dictionary<string, Vector3> From(List<ShapeData> data)
        {
            var map = new Dictionary<string, Vector3>();
            foreach (var shape in data)
                if (shape.Type == "Point")
                    map[shape.Id] = shape.Position;
            return map;
        }
    }

}