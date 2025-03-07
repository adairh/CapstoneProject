using UnityEngine;

public class Segment : Shape, IDrawable2D
{
    public Point Start { get; set; }
    public Point End { get; set; }
    private GameObject parent;

    public Segment(Point start, Point end, GameObject parent) : base(start.Position, "Segment")
    {
        Start = start;
        End = end;
        this.parent = parent;
        SetupGameObject();
    }

    private void SetupGameObject()
    {
        GO = new GameObject(Name);

        if (parent != null)
        {
            GO.transform.SetParent(parent.transform, false);
        }

        Draw2D();
    }

    public void Draw2D()
    {
        if (GO != null) 
            GameObject.Destroy(GO);

        GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        GO.name = Name;

        if (parent != null)
        {
            GO.transform.SetParent(parent.transform, false);
        }

        UpdateTransform();
    }

    public override void Draw()
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

    public override void OpenConfigPanel()
    {
        Debug.LogWarning($"{Name}: OpenConfigPanel() not implemented.");
    }

    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    }
}