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

        // TODO: Add CreateTorus() or custom meshes as needed
    }
}