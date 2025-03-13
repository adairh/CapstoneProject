using UnityEngine;

public class Point : Shape, IDrawable2D
{ 
    private int pointNO;

    public Point(Vector3 position) : this(position, null){
    }

    public Point(Vector3 position, Shape parent) : base(position, "Pivot " + AlphabetCounter.Next(), parent)
    {
        this.pointNO = AlphabetCounter.CurrentValue(); 
        SetupGameObject();
    }

    private void SetupGameObject()
    {
        GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GO.name = Name;
        GO.transform.localScale = Vector3.one * 0.1f; // Small size for a point

        if (Parent != null)
        {
            GO.transform.SetParent(Parent.GO.transform, true); // Preserve world position
            GO.transform.position = Position; // Ensure world position is correct
        }
        else
        {
            GO.transform.position = Position;
        }

        // ✅ Replace SphereCollider with BoxCollider for accuracy
        if (GO.GetComponent<SphereCollider>() != null)
        {
            GameObject.Destroy(GO.GetComponent<SphereCollider>());
        }
        var collider = GO.AddComponent<BoxCollider>();
        collider.size = Vector3.one * 0.1f;
    }

    public override void Drawing()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (GO == null) return;

        if (Parent != null)
        {
            GO.transform.SetParent(Parent.GO.transform, true); // Preserve world position
            GO.transform.position = Position; // Ensure world position is correct
        }
        else
        {
            GO.transform.position = Position;
        }
    }
    protected override void InitializeSettings()
    {
        //.LogWarning($"{Name}: InitializeSettings() not implemented.");
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

    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    } 
    
    public override void UpdateHitbox()
    {
    }
}
