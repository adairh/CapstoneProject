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

    public override void OpenConfigPanel()
    { 
    }

    public override void Draw()
    { 
    }
}
