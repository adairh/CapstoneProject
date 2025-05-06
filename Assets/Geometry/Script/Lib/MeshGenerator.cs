using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class MeshGenerator
    {
        public static Mesh CreateSphere(float radius, int segments = 16, int rings = 16)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            Mesh scaledMesh = Object.Instantiate(mesh);
            Vector3[] vertices = scaledMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] *= radius;
            scaledMesh.vertices = vertices;
            scaledMesh.RecalculateBounds();
            return scaledMesh;
        }

        public static Mesh CreateCylinder(float height, float radius, int segments = 20)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            Mesh scaledMesh = Object.Instantiate(mesh);
            Vector3[] vertices = scaledMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
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
            if (points == null || points.Count < 3)
            {
                Debug.LogError("[MeshGenerator] A plane requires at least 3 points.");
                return new Mesh();
            }

            Vector3[] vertices = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                vertices[i] = points[i].transform.position;
            }

            int[] triangles = { 0, 1, 2 };
            Vector3 normal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).normalized;
            Vector3[] normals = { normal, normal, normal };

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;

            return mesh;
        }
        
        // TODO: Add CreateTorus() or custom meshes as needed
    }
}