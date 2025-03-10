using UnityEngine;

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
        Debug.LogWarning($"{Name}: InitializeSettings() not implemented.");
    }

    public override void UpdateConfigData()
    {
        Debug.LogWarning($"{Name}: OpenConfigPanel() not implemented.");
    }

    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    }
    public override void ModifySetting<T>(ISetting setting, T value)
    { 
    }
}