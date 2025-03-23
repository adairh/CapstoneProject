using System.Collections.Generic;
using UnityEngine;

public class Segment : Shape, IDrawable2D
{
    public Point Start { get; set; }
    public Point End { get; set; }

    private static bool drawing = false;
    private static Vector3 startPoint;
    private static Segment currentSegment;

    public Segment(Point start, Point end, Shape parent) : base((start.Position), "Segment", parent)
    {
        Start = start;
        End = end;

        GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        GO.name = Name;

        if (Parent != null)
        {
            GO.transform.SetParent(Parent.GO.transform, false);
            Draw();
        }

        SetupGameObject();
    }


    public Segment(Point start, Point end) : this(start, end, null) { }

    private void SetupGameObject()
    {
        Draw2D();
    }
 

    public override void Drawing()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (GO == null) return;

        // ✅ Compute new segment offset
        Vector3 diff = Start.Position - End.Position;
        Vector3 offset = Position - Start.Position;

        // ✅ Move Start to the new Position and adjust End accordingly
        
        Debug.LogWarning($"New Position: {Position}");
        
        if (Parent == null)
        {
            Start.Position = Position;
            End.Position += offset;
        }

    // ✅ Compute new midpoint and length
        Vector3 midPoint = (Start.Position + End.Position) / 2;
        float length = diff.magnitude;
        if (length == 0)
        {
            length = 0.001f;
        }

        // ✅ Update GameObject Transform
        GO.transform.position = midPoint;
        GO.transform.localScale = new Vector3(0.05f, length / 2, 0.05f);
        GO.transform.rotation = Quaternion.FromToRotation(Vector3.up, End.Position - Start.Position);

        Start.Draw();
        End.Draw();
        
        // ✅ Now we update the collider AFTER the transform is changed
        //UpdateHitbox();
    }




    public static void Sketch(Vector3 worldPoint, Camera mainCamera)
    {
        if (Input.GetMouseButtonDown(0)) // Click to start drawing
        {
            if (!drawing)
            {

                if (HoverManager.Instance.GetPinnedShape() != null)
                {
                    Shape pin = HoverManager.Instance.GetPinnedShape();
                    if (pin is Point)
                    {
                        startPoint = pin.Position;
                        currentSegment = new Segment(((Point)pin), new Point(startPoint));
                    }
                    else
                    {
                        startPoint = worldPoint;
                        currentSegment = new Segment(new Point(startPoint), new Point(startPoint));
                    }
                }
                else
                {
                    startPoint = worldPoint;
                    currentSegment = new Segment(new Point(startPoint), new Point(startPoint));
                }
                // Start sketching by placing the first point
                drawing = true;
            }
            else
            {
                // Second click finalizes the segment
                currentSegment.End.Position = worldPoint;
                
                if (HoverManager.Instance.GetPinnedShape() != null)
                {
                    Shape pin = HoverManager.Instance.GetPinnedShape();
                    if (pin is Point)
                    {
                        currentSegment.End.Position = pin.Position;
                        currentSegment.End.Destroy();
                        currentSegment.End = ((Point)pin);
                    }
                    else
                    {
                        currentSegment.End.Position = worldPoint;
                    }
                }
                else
                {
                    currentSegment.End.Position = worldPoint;
                }
                
                currentSegment.UpdateTransform();
                currentSegment.CompleteDraw();
                drawing = false;
            }
        }

        if (drawing)
        {
            // Update the second point dynamically while dragging
            currentSegment.End.Position = worldPoint;
            currentSegment.Draw();
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
        gos.Add(GO);
        gos.Add(Start.GO);
        gos.Add(End.GO);
        
        return gos.ToArray();
    }

    public override void UpdateHitbox()
    {
        if (GO == null) return;

        // ✅ Ensure MeshCollider exists
        MeshCollider collider = GO.GetComponent<MeshCollider>();
        if (collider == null)
        {
            collider = GO.AddComponent<MeshCollider>();
        }

        // ✅ Force Unity to recalculate the mesh bounds
        MeshFilter meshFilter = GO.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.sharedMesh;
            mesh.RecalculateBounds(); // ✅ This ensures the collider matches the new shape
            collider.sharedMesh = null;  // ✅ Force Unity to refresh it
            collider.sharedMesh = mesh;  
        }

        collider.convex = false; // Keep non-convex for accuracy
    }


    public void Draw2D()
    {
    }

    public override void CompleteDraw()
    {
        UpdateHitbox();

        Vector3 loc = Position;

        GameObject go = new GameObject(Name);

        go.transform.position -= loc;
        
        Start.CompleteDraw();
        End.CompleteDraw();
        GO.transform.parent = go.transform;
        Start.GO.transform.parent = go.transform;
        End.GO.transform.parent = go.transform;
        
        // ✅ Ensure Points Keep Their Original Scale 

        base.CompleteDraw();
    }

}
