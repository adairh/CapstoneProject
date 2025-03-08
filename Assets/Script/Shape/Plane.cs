using UnityEngine;

public class Plane : Shape, IDrawable3D
{
    public Point[] points; // 3 points defining the plane 
    private float planeSizeLimit; // Limit size
    private System.Numerics.Plane sysPlane;

    public Plane(Point[] points, float limit) : this(points, limit, null)
    { }
    public Plane(Point[] points, float limit, Shape parent) : base(points[0].Position, "Plane", null)
    {
        planeSizeLimit = limit*2;
        sysPlane = new System.Numerics.Plane(); 
        if (points.Length != 3)
        {
            Debug.LogError("Plane needs exactly 3 points!");
            return;
        }

        this.points = points;
        SetupGameObject();
    }

    private void SetupGameObject()
    {
        GO = new GameObject("GeneratedPlane");
        GO.transform.position = GetMiddlePoint(points);

        if (Parent != null)
        {
            Parent.GO.name = Name + " " + id;
            GO.transform.SetParent(Parent.GO.transform, false);
        }

        Draw3D();
    }

    public Vector3 GetMiddlePoint(Point[] points)
    {
        return (points[0].Position + points[1].Position + points[2].Position) / 3;
    }

    public void Draw3D()
    {
        MeshFilter meshFilter = GO.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = GO.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = GO.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GO.AddComponent<MeshRenderer>();

        MeshCollider meshCollider = GO.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = GO.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();

        // **Step 1: Define Two Orthogonal Vectors**
        Vector3 vectorA = (points[1].Position - points[0].Position).normalized * planeSizeLimit;
        Vector3 normal = Vector3.Cross(vectorA, points[2].Position - points[0].Position).normalized; // Get normal
        Vector3 vectorB = Vector3.Cross(normal, vectorA).normalized * planeSizeLimit; // Ensure perpendicular vector

        // **Step 2: Compute Four Corners of the Plane**
        Vector3 center = GetMiddlePoint(points);
        Vector3 p0 = center - (vectorA / 2) - (vectorB / 2); // Bottom-left
        Vector3 p1 = center + (vectorA / 2) - (vectorB / 2); // Bottom-right
        Vector3 p2 = center - (vectorA / 2) + (vectorB / 2); // Top-left
        Vector3 p3 = center + (vectorA / 2) + (vectorB / 2); // Top-right

        // **Step 3: Assign Mesh Data**
        mesh.vertices = new Vector3[] { p0, p1, p2, p3 };  // Correct order
        mesh.triangles = new int[] { 
           // 0, 1, 3, 0, 3, 2, // Front Face
            0, 2, 3, 0, 3, 1  // Back Face
        };
        mesh.RecalculateNormals();

        // **Step 4: Apply Mesh**
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // **Step 5: Set Transparent Material**
        DefaultMaterial = new Material(Shader.Find("Ciconia Studio/CS_Standard/Builtin/Standard (Specular setup)/Opaque"));
        Material transparentMaterial = DefaultMaterial;
        

// Enable transparency
        transparentMaterial.SetFloat("_Mode", 3);
        transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMaterial.SetInt("_ZWrite", 0);
        transparentMaterial.DisableKeyword("_ALPHATEST_ON");
        transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
        transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        transparentMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        transparentMaterial.color = new Color(1f, 1f, 1f, 0.1f); // 30% Transparent
        meshRenderer.material = transparentMaterial;

    }




    protected override void InitializeSettings()
    {
        
    }

    public override void OpenConfigPanel()
    {
        
    }

    public override void Draw() => Draw3D();

    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    }
}
