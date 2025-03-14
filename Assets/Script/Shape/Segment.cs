﻿using UnityEngine;

public class Segment : Shape, IDrawable2D
{
    public Point Start { get; set; }
    public Point End { get; set; } 

    public Segment(Point start, Point end, Shape parent) : base(start.Position, "Segment", parent)
    {
        Start = start;
        End = end; 
        SetupGameObject();
    }

    public Segment(Point start, Point end) : this(start, end, null) { }

    private void SetupGameObject()
    {
        GO = new GameObject(Name);

        if (Parent != null)
        {
            GO.transform.SetParent(Parent.GO.transform, false);
        }

        Draw2D();
    }

    public void Draw2D()
    {
        if (GO != null)
            GameObject.Destroy(GO);

        GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        GO.name = Name;

        if (Parent != null)
        {
            GO.transform.SetParent(Parent.GO.transform, false);
        }

        UpdateTransform();
    
        // ✅ Replace the default collider with a CapsuleCollider
        if (GO.GetComponent<CapsuleCollider>() != null)
            GameObject.Destroy(GO.GetComponent<CapsuleCollider>());

        CapsuleCollider capsule = GO.AddComponent<CapsuleCollider>();
        capsule.direction = 1; // Align along the Y-axis
        capsule.radius = 0.025f; // Half of the thickness (0.05)
        capsule.height = Vector3.Distance(Start.Position, End.Position);
    }


    public override void Drawing()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (GO == null) return;

        Vector3 midPoint = (Start.Position + End.Position) / 2;
        float length = Vector3.Distance(Start.Position, End.Position);

        GO.transform.position = midPoint;
        GO.transform.localScale = new Vector3(0.05f, length / 2, 0.05f);
        GO.transform.rotation = Quaternion.FromToRotation(Vector3.up, End.Position - Start.Position);

        if (GO.GetComponent<Renderer>() != null)
        {
            GO.GetComponent<Renderer>().material = DefaultMaterial;
        }
    }

    protected override void InitializeSettings()
    {
        //Debug.LogWarning($"{Name}: InitializeSettings() not implemented.");
    }
    public override GameObject[] Components()
    {
        return new GameObject[]{}; // Use a List instead of an array
    }
    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    } 
    
    public override void UpdateHitbox()
    {
    }
}