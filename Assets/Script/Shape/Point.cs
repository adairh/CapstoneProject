using UnityEngine;

public class Point : Shape, IDrawable2D
{
    private GameObject parent;
    private int pointNO;

    public Point(Vector3 position, GameObject parent) : base(position, parent.name + " Pivot " + AlphabetCounter.Next())
    {
        this.pointNO = AlphabetCounter.CurrentValue();
        this.parent = parent;
        SetupGameObject();
    }

    private void SetupGameObject()
    {
        GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GO.name = Name;
        GO.transform.localScale = Vector3.one * 0.1f; // Small size for a point

        if (parent != null)
        {
            GO.transform.SetParent(parent.transform, true); // Preserve world position
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

    public override void Draw()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (GO == null) return;

        if (parent != null)
        {
            GO.transform.SetParent(parent.transform, true); // Preserve world position
            GO.transform.position = Position; // Ensure world position is correct
        }
        else
        {
            GO.transform.position = Position;
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
}
