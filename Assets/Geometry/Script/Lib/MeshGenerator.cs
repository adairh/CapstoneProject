using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manipulator
{
    public static class MeshGenerator
    {
        public static Mesh GenerateMesh(List<Vector3> points)
        {
            if (points.Count < 3) return null;

            var mesh = new Mesh();

            var basePos = points[0];
            var vertices = points.Select(p => p - basePos).ToArray();
            var tris = new List<int>();

            for (var i = 1; i < points.Count - 1; i++)
            {
                tris.Add(0);
                tris.Add(i);
                tris.Add(i + 1);
            }

            mesh.vertices = vertices;
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Move mesh back to world position
            for (var i = 0; i < vertices.Length; i++) vertices[i] += basePos;
            mesh.vertices = vertices;

            return mesh;
        }

        public static Mesh CreateSphere(float radius, int segments = 16, int rings = 16)
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            var scaledMesh = Object.Instantiate(mesh);
            var vertices = scaledMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
                vertices[i] *= radius;
            scaledMesh.vertices = vertices;
            scaledMesh.RecalculateBounds();
            return scaledMesh;
        }
        public static Mesh CreateCube(float radius, int segments = 16, int rings = 16)
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            var scaledMesh = Object.Instantiate(mesh);
            var vertices = scaledMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
                vertices[i] *= radius;
            scaledMesh.vertices = vertices;
            scaledMesh.RecalculateBounds();
            return scaledMesh;
        }

        public static Mesh CreateCylinder(float height, float radius, int segments = 20)
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            var scaledMesh = Object.Instantiate(mesh);
            var vertices = scaledMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                v.x *= radius;
                v.z *= radius;
                v.y *= height * 0.5f; // Unity's cylinder has height 2 by default
                vertices[i] = v;
            }

            scaledMesh.vertices = vertices;
            scaledMesh.RecalculateBounds();
            return scaledMesh;
        }

        public static Mesh CreatePlane(List<Point> points)
        {
            if (points.Count < 3) return null;

            var p0 = points[0].transform.position;
            var p1 = points[1].transform.position;
            var p2 = points[2].transform.position;

            // Tính hướng trục
            var dir1 = (p1 - p0).normalized;
            var dir2 = (p2 - p0).normalized;

            var extent1 = (p1 - p0).magnitude;
            var extent2 = (p2 - p0).magnitude;

            // 4 đỉnh mặt phẳng hình chữ nhật từ -1 đến +1 mỗi chiều
            var vertices = new Vector3[4]
            {
                -dir1 * extent1 - dir2 * extent2,
                dir1 * extent1 - dir2 * extent2,
                dir1 * extent1 + dir2 * extent2,
                -dir1 * extent1 + dir2 * extent2
            };

            int[] triangles =
            {
                0, 1, 2, // Mặt trước
                0, 2, 3, // Mặt trước
                2, 1, 0, // Mặt sau
                3, 2, 0 // Mặt sau
            };

            var mesh = new Mesh();
            mesh.name = "DoubleSidedPlane";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreatePlaneFacing(Vector3 axis1, Vector3 axis2, float extent1, float extent2)
        {
            axis1.Normalize();
            axis2.Normalize();

            var vertices = new Vector3[4]
            {
                -axis1 * extent1 - axis2 * extent2,
                axis1 * extent1 - axis2 * extent2,
                axis1 * extent1 + axis2 * extent2,
                -axis1 * extent1 + axis2 * extent2
            };

            int[] triangles =
            {
                0, 1, 2,
                0, 2, 3,
                2, 1, 0,
                3, 2, 0
            };

            var mesh = new Mesh();
            mesh.name = "PlaneFacing";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // TODO: Add CreateTorus() or custom meshes as needed
    }
}