using UnityEngine;

public static class PolygonMeshGenerator
{
    public static Mesh CreateExtrudedPolygon(Vector3[] vertices, float thickness = 0.01f)
    {
        var vertexCount = vertices.Length;

        if (vertexCount < 3)
        {
            Debug.LogError("Polygon must have at least 3 vertices!");
            return null;
        }

        var mesh = new Mesh();
        var extrudedVertices = new Vector3[vertexCount * 2];
        var triangles = new int[vertexCount * 6];

        // Compute normal direction (assumes coplanar points)
        var normal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).normalized;

        // Front & Back Faces
        for (var i = 0; i < vertexCount; i++)
        {
            extrudedVertices[i] = vertices[i]; // Front face
            extrudedVertices[i + vertexCount] = vertices[i] + normal * thickness; // Back face
        }

        // Generate triangles for front face
        for (var i = 0; i < vertexCount - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // Generate triangles for back face
        var offset = (vertexCount - 2) * 3;
        for (var i = 0; i < vertexCount - 2; i++)
        {
            triangles[offset + i * 3] = vertexCount + 0;
            triangles[offset + i * 3 + 1] = vertexCount + i + 2;
            triangles[offset + i * 3 + 2] = vertexCount + i + 1;
        }

        mesh.vertices = extrudedVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}