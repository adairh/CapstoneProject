using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Manipulator
{
    public static class ShapeFactory
    {
        private static readonly Dictionary<string, Func<Vector3, string, Shape>> creators = new()
        {
            { "Point", (pos, lgcName) => Create<Point>("Point", pos, lgcName) },
            { "Segment", (pos, lgcName) => Create<Segment>("Segment", pos, lgcName) },
            { "Line", (pos, lgcName) => Create<Line>("Line", pos, lgcName) },
            { "Ray", (pos, lgcName) => Create<RayShape>("Ray", pos, lgcName) },
            { "Polygon", (pos, lgcName) => Create<Polygon>("Polygon", pos, lgcName) },
            { "Plane", (pos, lgcName) => Create<PlaneShape>("Plane", pos, lgcName) }
        };

        public static Shape CreateShape(string type, Vector3 position, string lgcName = "")
        {
            //Debug.LogError($"[CreateShape {type}] {position}");

            if (creators.TryGetValue(type, out var ctor))
            {
                var instance = ctor(position, lgcName);
                //Debug.LogError($"[CreateShape {type}] {instance != null}");

                if (instance != null) return instance;
            }

            //Debug.LogError($"[ShapeFactory] Unknown shape type: {type}");
            return null;
        }


        public static T Create<T>(string type, Vector3 position, string lgcName = "") where T : Shape
        {
            var go = new GameObject(type);
            
            Volume volume = go.AddComponent<Volume>();
            volume.isGlobal = true; // Global Volume
            volume.priority = 100;  // High priority

            // Create a new Volume Profile
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Add Bloom effect
            Bloom bloom;
            if (!profile.TryGet(out bloom))
            {
                bloom = profile.Add<Bloom>(true);
            }
            bloom.intensity.value = 10f;      // Set intensity (adjust for your look)
            bloom.threshold.value = 4.5f;      // Threshold (lower = more glow)
            bloom.tint.value = new Color(0,5f, 1f, 0f);      
            bloom.active = true;

            // (Optional: tweak other bloom settings, e.g., scatter, clamp, etc.)

            // Assign profile to the volume
            volume.profile = profile;
            
            var shape = go.AddComponent<T>();
            Debug.Log($"[CreateShape {type}] Created shape: {shape}, type: {type}, id: {go.GetInstanceID()}");

            shape.InitializeNew(type, position, lgcName);
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
                var size = Vector3.zero;
                var center = Vector3.zero;
                var extent = 100f;
                var thickness = 0.1f;

                // Xác định hướng pháp tuyến của plane (giả sử shape.transform.forward là pháp tuyến)
                var normal = shape.transform.forward;

                if (Mathf.Abs(normal.x) > 0.9f)
                    // Nằm trên mặt OYZ
                    size = new Vector3(thickness, extent * 2, extent * 2);
                else if (Mathf.Abs(normal.y) > 0.9f)
                    // Nằm trên mặt OXZ
                    size = new Vector3(extent * 2, thickness, extent * 2);
                else
                    // Nằm trên mặt OXY
                    size = new Vector3(extent * 2, extent * 2, thickness);

                box.size = size;
                box.center = center;
            }

            return shape;
        }


        public static Shape CreateFromData(ShapeData data)
        {
            //Debug.LogError($"[CreateFromData] {data.Id}");

            //Debug.LogError($"[Click] Create {data.Id} {data.Type} {data.Position}");
            if (ShapeStorage.GetById(data.Id) != null) return null;


            var shape = CreateShape(data.Type, data.Position, data.LogicalName);
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