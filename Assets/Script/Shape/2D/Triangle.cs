using UnityEngine;

public class Triangle : PolygonalShape, IDrawable2D
{
    public Point[] Corners { get; private set; } 
    private GameObject[] edges;

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3) : this(p1, p2, p3, null)
    {
    }

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, Shape parent) : base(p1, "Triangle", parent)
    {
        GO = new GameObject(Name);

        // ✅ Calculate center for proper positioning
        Vector3 center = (p1 + p2 + p3) / 3;
        Position = center;
        GO.transform.position = center;

        // ✅ Initialize points as children of the triangle GOect
        Corners = new Point[]
        {
            new Point(p1, this),
            new Point(p2, this),
            new Point(p3, this)
        };

        SetupGameObject();
    }

    private void SetupGameObject()
    {
        // ✅ Add interactivity components
        /*GO.AddComponent<DraggableShape>();
        GO.AddComponent<ScalableShape>();
        GO.AddComponent<RotatableShape>();
        HoverableShape hover = GO.AddComponent<HoverableShape>();
        hover.SetMaterials(DefaultMaterial, HighlightMaterial);*/

        // ✅ Fix the PolygonCollider2D to match triangle shape
        PolygonCollider2D collider = GO.AddComponent<PolygonCollider2D>();
        collider.points = GetLocalPoints(); 

        // ✅ Create edges
        edges = new GameObject[3]; 
    }

    private Vector2[] GetLocalPoints()
    {
        return new Vector2[]
        {
            GO.transform.InverseTransformPoint(Corners[0].Position),
            GO.transform.InverseTransformPoint(Corners[1].Position),
            GO.transform.InverseTransformPoint(Corners[2].Position)
        };
    }

    public void Draw2D()
    {
        /*CreateEdge(0, Corners[0].GetGameObject().transform.localPosition, Corners[1].GetGameObject().transform.localPosition);
        CreateEdge(1, Corners[1].GetGameObject().transform.localPosition, Corners[2].GetGameObject().transform.localPosition);
        CreateEdge(2, Corners[2].GetGameObject().transform.localPosition, Corners[0].GetGameObject().transform.localPosition);
    */
        
        new Segment(Corners[0], Corners[1], this);
        new Segment(Corners[1], Corners[2], this);
        new Segment(Corners[2], Corners[0], this);
    }

    private void CreateEdge(int index, Vector3 localStart, Vector3 localEnd)
    {
        if (edges[index] != null) GameObject.Destroy(edges[index]);

        edges[index] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        edges[index].transform.SetParent(GO.transform, false);

        Vector3 midPoint = (localStart + localEnd) / 2;
        float length = Vector3.Distance(localStart, localEnd);

        edges[index].transform.localPosition = midPoint; // ✅ Now local space
        edges[index].transform.localScale = new Vector3(0.05f, length / 2, 0.05f); // ✅ Thin cylinder
        edges[index].transform.rotation = Quaternion.FromToRotation(Vector3.up, localEnd - localStart);
        edges[index].GetComponent<Renderer>().material = DefaultMaterial;
    }

    public override void Drawing() => Draw2D();
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
