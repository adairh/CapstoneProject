using UnityEngine;

public class MeshRuntimeUpdater : MonoBehaviour
{
    public Transform[] points;    // Assign this with the spawned point transforms, in correct order
    public int[] triangles;       // Same triangle array you use in MeshCompute

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Vector3[] lastPositions;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        lastPositions = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
            lastPositions[i] = points[i].position;
    }

    void Update()
    {
        // If any point is destroyed, clean up mesh and destroy this component
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
            {
                // Destroy mesh and this component
                if (meshFilter && meshFilter.sharedMesh != null)
                {
                    Destroy(meshFilter.sharedMesh);
                    meshFilter.sharedMesh = null;
                }
                if (meshCollider)
                    meshCollider.sharedMesh = null;

                Destroy(this);
                return;
            }
        }

        bool changed = false;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].position != lastPositions[i])
            {
                changed = true;
                lastPositions[i] = points[i].position;
            }
        }
        if (changed)
            UpdateMesh();
    }

    void UpdateMesh()
    {
        if (meshFilter == null || meshCollider == null) return;
        var mesh = new Mesh();
        mesh.vertices = System.Array.ConvertAll(points, p => p.position);
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
