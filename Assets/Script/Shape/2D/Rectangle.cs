using UnityEngine;

public class Rectangle : PolygonalShape, IDrawable2D
{
    public Point[] Corners { get; private set; } 
    public float Width { get; set; }
    public float Height { get; set; }
    private Segment[] edges; // Use Segment instead of Shape for clarity

    private Vector3 bottomLeft;

    public Rectangle(Vector3 bottomLeft, float width, float height)
        : base(bottomLeft, "Rectangle")
    {
        GO = new GameObject(Name);

        this.bottomLeft = bottomLeft;
        Width = width;
        Height = height;

        // ✅ Calculate the center for positioning
        Position = bottomLeft + new Vector3(width / 2, height / 2, 0);
        GO.transform.position = Position;

        InitializeCorners();
        InitializeEdges();
        SetupGameObject();
    }

    private void InitializeCorners()
    {
        Corners = new Point[]
        {
            new Point(bottomLeft, GO),
            new Point(bottomLeft + new Vector3(Width, 0, 0), GO),
            new Point(bottomLeft + new Vector3(Width, Height, 0), GO),
            new Point(bottomLeft + new Vector3(0, Height, 0), GO)
        };
    }

    private void InitializeEdges()
    {
        edges = new Segment[]
        {
            new Segment(Corners[0], Corners[1], GO),
            new Segment(Corners[1], Corners[2], GO),
            new Segment(Corners[2], Corners[3], GO),
            new Segment(Corners[3], Corners[0], GO)
        };
    }

    private void SetupGameObject()
    {
        // ✅ Add BoxCollider2D and ensure correct size
        BoxCollider2D collider = GO.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(Width, Height);
        collider.offset = Vector2.zero;

        Draw2D();
    }

    public void Draw2D()
    {
        Debug.Log($"✅ Drawing Rectangle with Cylinders at {Position}");
    }

    public override void Draw()
    {
        // ✅ Update corners dynamically
        for (int i = 0; i < Corners.Length; i++)
        {
            Corners[i].Position = bottomLeft + new Vector3(
                (i == 1 || i == 2) ? Width : 0,
                (i >= 2) ? Height : 0,
                0
            );
            Corners[i].Draw();
        }

        // ✅ Update edges dynamically
        for (int i = 0; i < edges.Length; i++)
        {
            edges[i].Start = Corners[i];
            edges[i].End = Corners[(i + 1) % Corners.Length]; // Loop back to the first point
            edges[i].Draw();
        }
    }

    protected override void InitializeSettings()
    {
        // Placeholder for future settings
    }

    public override void OpenConfigPanel()
    {
        // Placeholder for UI configuration
    }
}
