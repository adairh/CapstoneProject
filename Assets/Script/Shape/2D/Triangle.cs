using UnityEngine;

public class Triangle : PolygonalShape, IDrawable2D
{
    public Point[] Corners { get; private set; }
    private GameObject[] edges;

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3) : this(p1, p2, p3, null) { }

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, Shape parent) : base(p1, "Triangle", parent)
    {
        GO = new GameObject(Name);

        // ✅ Calculate center for proper positioning
        Vector3 center = (p1 + p2 + p3) / 3;
        Position = center;
        GO.transform.position = center;

        // ✅ Initialize points as children of the triangle
        Corners = new Point[]
        {
            new Point(p1, this),
            new Point(p2, this),
            new Point(p3, this)
        };

        SetupGameObject();
    }

    public override void UpdateHitbox()
    {
    }

    private void SetupGameObject()
    {
        // ✅ Add MeshCollider (Non-Convex Mode)
        MeshCollider meshCollider = GO.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = CreateTriangleMesh();
        meshCollider.convex = false; // ❌ Do NOT make it convex (Triangles are flat!)

        // ✅ Create edges
        edges = new GameObject[3];
        Draw2D();
    }


    private Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            GO.transform.InverseTransformPoint(Corners[0].Position),
            GO.transform.InverseTransformPoint(Corners[1].Position),
            GO.transform.InverseTransformPoint(Corners[2].Position)
        };

        int[] triangles = { 0, 1, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    public void Draw2D()
    {
        CreateEdge(0, Corners[0].Position, Corners[1].Position);
        CreateEdge(1, Corners[1].Position, Corners[2].Position);
        CreateEdge(2, Corners[2].Position, Corners[0].Position);
    }

    private void CreateEdge(int index, Vector3 start, Vector3 end)
    {
        if (edges[index] != null) GameObject.Destroy(edges[index]);

        edges[index] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        edges[index].transform.SetParent(GO.transform, false);

        Vector3 midPoint = (start + end) / 2;
        float length = Vector3.Distance(start, end);

        edges[index].transform.position = midPoint;
        edges[index].transform.localScale = new Vector3(0.05f, length / 2, 0.05f);
        edges[index].transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
        edges[index].GetComponent<Renderer>().material = DefaultMaterial;
    }

    public override void Drawing() => Draw2D();

    protected override void InitializeSettings()
    {
        AppendSettings(
            new PositionSetting(Position, this)
        );
    }

    public override GameObject[] Components() => edges;

    // 🎨 Dynamic Sketching Support
    private static bool drawing = false;
    private static Vector3[] points = new Vector3[3];
    private static int pointCount = 0;
    private static Triangle triangle;

    public static void Sketch(Vector3 vector3, Vector3 screenPoint, Camera mainCamera)
    {
        if (Input.GetMouseButtonDown(0)) // Click to start
        {
            if (!drawing)
            {
                drawing = true;
                pointCount = 0;
            }

            if (pointCount < 3)
            {
                points[pointCount] = vector3;
                pointCount++;
            }

            if (pointCount == 3) // Complete triangle
            {
                triangle = new Triangle(points[0], points[1], points[2]);
                triangle.GO.transform.rotation = GetAlignedRotation(mainCamera);
                triangle.CompleteDraw();
                drawing = false;
            }
        }
    }
 
}
