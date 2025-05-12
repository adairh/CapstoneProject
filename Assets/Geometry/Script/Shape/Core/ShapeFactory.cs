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
            {"Segment", pos => Create<Segment>("Segment", pos)},
            {"Line", pos => Create<Line>("Line", pos)},
            {"Ray", pos => Create<RayShape>("Ray", pos)},
            {"Polygon", pos => Create<Polygon>("Polygon", pos)},
            {"Plane", pos => Create<PlaneShape>("Plane", pos)}
        };

        public static Shape CreateShape(string type, Vector3 position)
        {
            Debug.LogError($"[CreateShape {type}] {position}");

            if (creators.TryGetValue(type, out var ctor))
            {
                var instance = ctor(position);
                Debug.LogError($"[CreateShape {type}] {instance != null}");

                if (instance != null) return instance;
            }

            //Debug.LogError($"[ShapeFactory] Unknown shape type: {type}");
            return null;
        }


        public static T Create<T>(string type, Vector3 position) where T : Shape
        {
            var go = new GameObject(type);
            var shape = go.AddComponent<T>();
            Debug.Log($"[CreateShape {type}] Created shape: {shape}, type: {type}, id: {go.GetInstanceID()}");

            shape.InitializeNew(type, position);
            return shape;
        }


        
        private static T CreateWithCollider<T>(string type, Vector3 position) where T : Shape
        {
            var shape = Create<T>(type, position);

            if (shape is PlaneShape plane)
            {
                var box = shape.gameObject.AddComponent<BoxCollider>();
                box.isTrigger = false;

                // Ngay lập tức gán collider chính xác
                Vector3 size = Vector3.zero;
                Vector3 center = Vector3.zero;
                float extent = 100f;
                float thickness = 0.1f;

                // Xác định hướng pháp tuyến của plane (giả sử shape.transform.forward là pháp tuyến)
                Vector3 normal = shape.transform.forward;

                if (Mathf.Abs(normal.x) > 0.9f)
                {
                    // Nằm trên mặt OYZ
                    size = new Vector3(thickness, extent * 2, extent * 2);
                }
                else if (Mathf.Abs(normal.y) > 0.9f)
                {
                    // Nằm trên mặt OXZ
                    size = new Vector3(extent * 2, thickness, extent * 2);
                }
                else
                {
                    // Nằm trên mặt OXY
                    size = new Vector3(extent * 2, extent * 2, thickness);
                }

                box.size = size;
                box.center = center;
            }

            return shape;
        }

        
        
        public static Shape CreateFromData(ShapeData data)
        {
            //Debug.LogError($"[CreateFromData] {data.Id}");

            if (ShapeStorage.GetById(data.Id) != null) return null;
            
            var shape = CreateShape(data.Type, data.Position);
            //Debug.LogError($"[CreateFromData] {shape != null}");

            if (shape == null) return null;
            
            // ❗ Unregister tạm vì shape đang nằm dưới ShapeId cũ
            ShapeStorage.Unregister(shape);
            
            // ⬇ Gán đúng ID và dữ liệu từ server
            shape.Deserialize(data);
            
            // ✅ Re-register lại đúng ShapeId
            ShapeStorage.Register(shape);
            
            return shape;

        }
    }
}