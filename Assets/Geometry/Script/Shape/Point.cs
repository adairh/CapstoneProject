using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class Point : Shape, IDrawable2D
{
    private int pointNO;
    private SphereCollider collider;
    private Constraint constraint = new FixedPointConstraint(); // Composition
    private HashSet<Shape> attachedShapes = new HashSet<Shape>(); 

    public Point(Vector3 position) : this(position, null) { }

    public Point(Vector3 position, Shape parent) : base(position, "Pivot " + AlphabetCounter.Next(), parent)
    {
        this.pointNO = AlphabetCounter.CurrentValue();
        SetupGameObject();
        
        
        
        
        
    }

    private NetworkObject segmentNetObj = null;
    
    private void SetupGameObject()
    {
        GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GO.name = Name;
        GO.transform.localScale = Vector3.one * 0.1f;
        GO.transform.position = Position;

        if (Parent != null)
            GO.transform.SetParent(Parent.GO.transform, true);

        collider = GO.GetComponent<SphereCollider>() ?? GO.AddComponent<SphereCollider>();
        UpdateHitbox();

        ConstraintManager.Instance.RegisterConstraint(constraint);
        constraint.AddShape(this);

        
    }

    public override void Destroy()
    {
        base.Destroy();
    }
    
    public void AttachProcess()
    {
        var mm = ManipulationManager.Instance;
        var shape = mm.GetPinnedShape();
        
        if (shape != null && shape != this && !(shape is Point))
        {
            shape.AddDepend(this);
        }
    }

    public override void Drawing()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (GO == null) return;

        GO.transform.position = Position;
        GO.transform.localScale = Vector3.one * 0.1f;

        // Notify shapes that depend on this point
        foreach (var shape in attachedShapes)
        {
            shape.OnPointMoved(this);
        }
    }

    public void MoveTo(Vector3 newPosition)
    {
        Position = newPosition;
        UpdateTransform();
    }

    public override void OnPointMoved(Point movedPoint)
    {
        // Ignore
    }

    public override void UpdateHitbox()
    {
        if (collider == null) return;
        collider.center = Vector3.zero;
    }

    protected override void InitializeSettings()
    {
        AppendSettings(new PositionSetting(Position, this));
    }

    public override GameObject[] Components()
    {
        return new[] { GO };
    }

    public void Draw2D() { }

    public override void CompleteDraw()
    {
        UpdateHitbox();
        base.CompleteDraw();
    }

    public void AttachToShape(Shape shape)
    {
        if (!attachedShapes.Contains(shape))
            attachedShapes.Add(shape);
    }
}

}