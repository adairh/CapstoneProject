using System.Collections.Generic;
using UnityEngine;

public class Triangle : PolygonalShape, IDrawable2D
{
    public Point[] Corners { get; private set; }
    private Segment[] edges;

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
        InitializeEdges();
        SetupGameObject();
    }

    public override void UpdateHitbox()
    {
        if (GO == null) return;

        MeshCollider meshCollider = GO.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = GO.AddComponent<MeshCollider>();

        // Generate mesh in local space
        Vector3[] localCorners = new Vector3[]
        {
            GO.transform.InverseTransformPoint(Corners[0].Position),
            GO.transform.InverseTransformPoint(Corners[1].Position),
            GO.transform.InverseTransformPoint(Corners[2].Position)
        };

        meshCollider.sharedMesh = PolygonMeshGenerator.CreateExtrudedPolygon(localCorners, 0.01f);

        meshCollider.convex = false; // Keep it non-convex for accurate collisions
    }


    private void SetupGameObject()
    {
        
        Draw2D();
    }
 

    public void Draw2D()
    {
         
    }

    private void InitializeEdges()
    {
        edges = new Segment[]
        {
            new Segment(Corners[0], Corners[1], this),
            new Segment(Corners[1], Corners[2], this),
            new Segment(Corners[2], Corners[0], this)
        };
    }

    public override void Drawing()
    {
        Quaternion rotation = GO.transform.rotation;  // Get current rotation
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;

        Vector3 new_center = Position;
        Vector3 old_center = (Corners[0].Position + Corners[0].Position + Corners[0].Position) / 3;

        Vector3 gap = new_center - old_center;
        
        for (int i = 0; i < Corners.Length; i++)
        {
            Corners[i].Position += gap;
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
    
    // 🎨 Dynamic Sketching Support
    private static bool drawing = false;
    private static Vector3[] points = new Vector3[3];
    private static int pointCount = 0;
    private static Triangle triangle;
    private static Vector3 startScreenPoint;

    public static void Sketch(Vector3 vector3, Vector3 screenPoint, Camera mainCamera)
    {
        if (Input.GetMouseButtonDown(0)) // Click to place points
        {
            if (!drawing)
            {
                // First click: Initialize the triangle with the first point
                drawing = true;
                pointCount = 1;
                points = new Vector3[3];

                points[0] = vector3;
                points[1] = vector3; // Initialize second point at first point
                points[2] = vector3; // Initialize third point at first point
                triangle = new Triangle(points[0], points[1], points[2]); 
            }
            else
            {
                if (pointCount == 1)
                {
                    // Second click: Lock the second point
                    //points[1] = vector3;
                    pointCount++;
                }
                else if (pointCount == 2)
                {
                    // Third click: Lock the third point and finalize the shape
                    /*points[2] = vector3;
                    triangle.Corners[0].Position = points[0];
                    triangle.Corners[1].Position = points[1];
                    triangle.Corners[2].Position = points[2];
                    */

                    // Align to camera and finalize drawing
                    triangle.CompleteDraw();
                    drawing = false;
                }
            }
        }

        if (drawing)
        {
            // While dragging, update the next point dynamically
            if (pointCount == 1)
            {
                points[1] = vector3; // Update second point dynamically
            }
            else if (pointCount == 2)
            {
                points[2] = vector3; // Update third point dynamically
            }

            // Update triangle corners in real time
            triangle.Corners[0].Position = points[0];
            triangle.Corners[1].Position = points[1];
            triangle.Corners[2].Position = points[2];

            triangle.GO.transform.rotation = GetAlignedRotation(mainCamera);
            
            triangle.Draw();
        }
    }


    public override void CompleteDraw()
    {
        UpdateHitbox();
        base.CompleteDraw();
    }


 
}
