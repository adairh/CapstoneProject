using UnityEngine;

public class Pyramid : PolygonalShape, IDrawable3D
{

    public Pyramid(Vector3 position) : this(position, null)
    {
    }

    public Pyramid(Vector3 position, Shape parent) : base(position, "Pyramid", parent)
    {
        Draw3D();
    }

    public void Draw3D()
    {
        if (GO == null)
        {
            GO = new GameObject(Name);
            GO.transform.position = Position;

            MeshFilter meshFilter = GO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = GO.AddComponent<MeshRenderer>();
            MeshCollider collider = GO.AddComponent<MeshCollider>();

            meshFilter.mesh = CreatePyramidMesh();
            collider.sharedMesh = meshFilter.mesh;

            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = ShapeColor;

            // Add interaction scripts
            /*GO.AddComponent<DraggableShape>();
            GO.AddComponent<ScalableShape>();
            GO.AddComponent<RotatableShape>();
            var hover = GO.AddComponent<HoverableShape>();
            hover.SetMaterials(DefaultMaterial, HighlightMaterial);*/
        }
    }

    private Mesh CreatePyramidMesh()
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, 1, 0),  // Top vertex
            new Vector3(-0.5f, 0, -0.5f), // Base 1
            new Vector3(0.5f, 0, -0.5f),  // Base 2
            new Vector3(0.5f, 0, 0.5f),   // Base 3
            new Vector3(-0.5f, 0, 0.5f)   // Base 4
        };

        int[] triangles = new int[]
        {
            0, 1, 2, // Side 1
            0, 2, 3, // Side 2
            0, 3, 4, // Side 3
            0, 4, 1, // Side 4
            1, 4, 3, 3, 2, 1 // Base
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    public override void Drawing() => Draw3D();
    protected override void InitializeSettings()
    {
        //throw new System.NotImplementedException();
    }
 
    public override void ModifySetting<T>(ISetting setting, T value)
    { 
    }
    public override void UpdateConfigData()
    {
        //throw new System.NotImplementedException();
    }

    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    }
}