using Manipulator;
using UnityEngine;

namespace Geometry.Script.Network
{
    public static class ShapeFactory
    {
        public static Shape CreateFromJson(string type, string json)
        {
            switch (type)
            {
                case "Circle": return JsonUtility.FromJson<Circle>(json);
                case "Segment": return JsonUtility.FromJson<Segment>(json);
                // Add more here
                default: return null;
            }
        }
    }

}