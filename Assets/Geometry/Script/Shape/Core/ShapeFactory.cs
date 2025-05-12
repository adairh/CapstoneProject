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
            {"Plane", pos => CreateWithCollider<PlaneShape>("Plane", pos)}
        };

        public static Shape CreateShape(string type, Vector3 position)
        {
            //Debug.LogError($"[CreateShape] {position}");

            if (creators.TryGetValue(type, out var ctor))
            {
                var instance = ctor(position);
                //Debug.LogError($"[CreateShape] {instance != null}");

                if (instance != null) return instance;
            }

            //Debug.LogError($"[ShapeFactory] Unknown shape type: {type}");
            return null;
        }


        public static T Create<T>(string type, Vector3 position) where T : Shape
        { 
            var go = new GameObject(type);
            var shape = go.AddComponent<T>();
            shape.InitializeNew(type, position);
            if (type == "Plane") return shape;
            
            var drag = go.AddComponent<DraggableShape>();
            if (drag == null)
            {
                //Debug.LogError($"[Create] Failed to add DraggableShape to {type}");
            }
            else
            {
                drag.SetShape(shape);
            }

 
            return shape;
        }

        
        private static T CreateWithCollider<T>(string type, Vector3 position) where T : Shape
        {
            var shape = Create<T>(type, position);

            if (shape is PlaneShape plane)
            {
                var box = shape.gameObject.AddComponent<BoxCollider>();
                box.isTrigger = false;

                plane.OnMeshUpdated += mesh =>
                {
                    var bounds = mesh.bounds;
                    var scale = shape.transform.localScale;

                    Vector3 size = Vector3.Scale(bounds.size, scale);
                    Vector3 center = bounds.center;

                    // Xác định hướng pháp tuyến để làm collider thật mỏng ở trục đó
                    Vector3 normal = shape.transform.forward;
                    float thin = 0.01f;

                    Vector3 colliderSize = size;

                    if (Mathf.Abs(normal.x) > 0.9f)
                        colliderSize = new Vector3(thin, size.y*2, size.z);  // Nằm trong OYZ
                    else if (Mathf.Abs(normal.y) > 0.9f)
                        colliderSize = new Vector3(size.x, thin, size.z);  // Nằm trong OXZ
                    else
                        colliderSize = new Vector3(size.x, size.y*2, thin);  // Nằm trong OXY

                    box.size = colliderSize;
                    box.center = center;
                };
            }/*
            else
            {
                var meshCollider = shape.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.isTrigger = false;

                if (shape is MeshBasedShape meshShape)
                {
                    meshShape.OnMeshUpdated += mesh => meshCollider.sharedMesh = mesh;
                }
            }*/

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