using UnityEngine;

public class Point : Shape, IDrawable2D
{ 
    private int pointNO;
    private SphereCollider collider;

    public Point(Vector3 position) : this(position, null) { }

    public Point(Vector3 position, Shape parent) : base(position, "Pivot " + AlphabetCounter.Next(), parent)
    {
        this.pointNO = AlphabetCounter.CurrentValue();
        SetupGameObject();
    }

    private void SetupGameObject()
    {
        GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GO.name = Name;
        GO.transform.localScale = Vector3.one * 0.1f; // Small point

        if (Parent != null)
        {
            GO.transform.SetParent(Parent.GO.transform, true); // Preserve world position
            GO.transform.position = Position; // Ensure world position is correct
        }
        else
        {
            GO.transform.position = Position;
        }

        // ✅ Ensure precise SphereCollider
        collider = GO.GetComponent<SphereCollider>();
        if (collider == null) collider = GO.AddComponent<SphereCollider>();
        Drawing();
        UpdateHitbox(); // Ensure hitbox is properly set
    }

    public override void UpdateHitbox()
    {
        if (collider == null) return;

        float worldScale = GO.transform.localScale.x; // Get real world size
        //collider.radius = worldScale; // Set radius correctly
        collider.center = Vector3.zero; // Keep it centered
    }


    public override void Drawing()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (GO == null) return;

        
            GO.transform.position = Position;
            GO.transform.localScale = Vector3.one * 0.1f; // Keep normal size
        
    }


    protected override void InitializeSettings()
    {
        // LogWarning($"{Name}: InitializeSettings() not implemented.");
    }

    public override GameObject[] Components()
    {
        return new GameObject[]{}; // Use a List instead of an array
    }

    public GameObject GetGameObject()
    {
        return GO;
    }

    public void Draw2D()
    {
        Debug.Log($"{Name} is being drawn in 2D.");
    }

    
    public override void CompleteDraw()
    {
        UpdateHitbox();
        base.CompleteDraw();
    }
}
