using System.Collections.Generic;
using Unity.Netcode; 
using UnityEngine;

namespace Manipulator
{
    public class Point : Shape
{
    private int pointNO;
    private SphereCollider collider;
    
    private FixedPointConstraint constraint; // Composition 

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

        constraint = GO.AddComponent<FixedPointConstraint>();
        constraint.Owner = this;
        ConstraintManager.Instance.RegisterConstraint(constraint);
        
        

        
    }

    public override void Destroy()
    {
        base.Destroy();
    }

    public FixedPointConstraint GetPointConstraint() => constraint;
    
    public void AttachProcess()
    {
        var mm = ManipulationManager.Instance;
        var shape = mm.GetPinnedShape();
            
        
        if (shape != null && shape != this && !(shape is Point))
        {
            constraint.AddDepend(this, shape);
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
        constraint.ApplyConstraint(this, new Vector3());
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
        if (!constraint.GetLinkedShapes().Contains(shape))
            constraint.AddShape(shape);
    }

    public List<Shape> AttachedShapes => constraint.GetLinkedShapes();
}

}