using System.Collections.Generic;
using ThunderFireUnityEx;
using UnityEngine;

public class Rectangle : PolygonalShape, IDrawable2D
{
    public Point[] Corners { get; private set; } 
    public float Width { get; set; }
    public float Height { get; set; }
    private Segment[] edges; // Use Segment instead of Shape for clarity

    private Vector3 bottomLeft; 

    public Rectangle(Vector3 bottomLeft, float width, float height) : this(bottomLeft, width, height, null)
    {
    }

    public Rectangle(Vector3 bottomLeft, float width, float height, Shape parent)
        : base(bottomLeft, "Rectangle", parent)
    {
        GO = new GameObject(Name);

        Width = width;
        Height = height;

        // ✅ Calculate the center for positioning
        Position = bottomLeft + new Vector3(width / 2, height / 2, 0);
        GO.transform.position = Position;

        this.bottomLeft = Position;
        
        InitializeCorners();
        InitializeEdges();
        SetupGameObject();
    }

    private void InitializeCorners()
    {
        Quaternion rotation = GO.transform.rotation;  // Get the GameObject's rotation

        Vector3 right = rotation * Vector3.right;  // Rotated X-axis
        Vector3 up = rotation * Vector3.up;  // Rotated Y-axis

        Corners = new Point[]
        {
            new Point(bottomLeft, this),
            new Point(bottomLeft + right * Width, this),
            new Point(bottomLeft + right * Width + up * Height, this),
            new Point(bottomLeft + up * Height, this)
        };
    }


    private void InitializeEdges()
    {
        edges = new Segment[]
        {
            new Segment(Corners[0], Corners[1], this),
            new Segment(Corners[1], Corners[2], this),
            new Segment(Corners[2], Corners[3], this),
            new Segment(Corners[3], Corners[0], this)
        };
    }

    protected override void SetupGameObject()
    {
        Draw2D();
        UpdateHitbox();
        base.SetupGameObject();
    }

    public void Draw2D()
    {
        Debug.Log($"✅ Drawing Rectangle with Cylinders at {Position}");
    }
 

    public override void Drawing()
    {
        Quaternion rotation = GO.transform.rotation;  // Get current rotation
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;

        for (int i = 0; i < Corners.Length; i++)
        {
            Corners[i].Position = bottomLeft + 
                                  ((i == 1 || i == 2) ? 
                                      right * Width : Vector3.zero) + 
                                  ((i >= 2) ? 
                                      up * Height : Vector3.zero);
            Corners[i].Draw();
        }

        // ✅ Update edges to connect rotated corners
        for (int i = 0; i < edges.Length; i++)
        {
            edges[i].Start = Corners[i];
            edges[i].End = Corners[(i + 1) % Corners.Length]; // Loop back to first corner
            edges[i].Draw();
        }
    }


    protected override void InitializeSettings()
    {
        AppendSettings(
            new PositionSetting(Position, this)
        );
    }
    public override GameObject[] Components()
    {
        List<GameObject> gos = new List<GameObject>(); // Use a List instead of an array

        foreach (Shape s in edges)
        {
            if (s != null) // Ensure s is not null
            {
                gos.Add(s.GO); // Assuming Shape has a gameObject property
            }
        }
        
        foreach (Point p in Corners)
        {
            if (p != null) // Ensure s is not null
            {
                gos.Add(p.GO); // Assuming Shape has a gameObject property
            }
        }

        return gos.ToArray(); // Convert the List to an array
    }

    private static bool drawing = false;
    private static Vector3 startPoint;  
    private static Rectangle rect;
    private static Vector3 startScreenPoint;
    public static void Sketch(Vector3 hitPoint, Vector3 screenPoint, Camera mainCamera)
    {
        if (Input.GetMouseButtonDown(0)) // Click to start
        {
            if (!drawing)
            {
                startPoint = hitPoint;
                startScreenPoint = screenPoint;
                rect = new Rectangle(startPoint, 0, 0);
                drawing = true;
            }
        }
        else if (drawing && Input.GetMouseButton(0)) // Hold to resize
        {
            Vector3 size = (screenPoint - startScreenPoint)/100;
            
            float newWidth = size.x;
            float newHeight = size.y;
 
            
            if (!Mathf.Approximately(rect.Width, newWidth) || !Mathf.Approximately(rect.Height, newHeight))
            {
                rect.Width = newWidth;
                rect.Height = newHeight;
            
                Vector3 newPos = startPoint + new Vector3(size.x / 2, 0, size.z / 2);
                rect.Position = newPos;
            
                rect.GO.transform.rotation = GetAlignedRotation(mainCamera); // Rotate based on camera
                rect.Draw();
            }
            
        }
        else if (Input.GetMouseButtonUp(0)) // Release to finalize
        {
            if (drawing)
            {
                drawing = false;
                rect.CompleteDraw();
            }
        }
    }
    public override void UpdateHitbox()
    {
         
        BoxCollider boxCollider = GO.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = GO.AddComponent<BoxCollider>();
        }
    
        // ✅ Set correct size using width & height, assuming Z-depth is small
        boxCollider.size = new Vector3(Width, Height, 0.1f); 
    
        // ✅ Offset to center the collider properly
        boxCollider.center = new Vector3(Width / 2, Height / 2, 0);
    }


    public override void CompleteSettings()
    {
        bottomLeft = Corners[0].GO.transform.position;
        base.CompleteSettings();
    }

    public override void CompleteDraw()
    {
        UpdateHitbox();
        base.CompleteDraw();
    }
}
