using UnityEngine;

public class GroupShape : Shape
{
    public GroupShape(Shape parent) : base(new Vector3(0, 0, 0), "Group", parent)
    {
        if (GO == null)
        {
            GO = new GameObject("Group");
        }
    }
    
    public GroupShape() : this(null)
    {
    }

    protected override void InitializeSettings()
    { 
    }
    public override GameObject[] Components()
    {
        return new GameObject[]{}; // Use a List instead of an array
    }

    public override void Drawing()
    { 
    } 
    
    public override void UpdateHitbox()
    {
    }
}
