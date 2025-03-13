using UnityEngine;

public class HoverableShape : MonoBehaviour
{
    private Renderer renderer;
    public Shape _shape;
    private GameObject[] shapeComponents; // Store all components of the shape

    public void SetMaterials(Shape shape)
    {
        _shape = shape;
        

        renderer = GetComponent<Renderer>();
        if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _shape.DefaultMaterial;
    }

    public void SetComponents()
    {
        shapeComponents = _shape.Components(); // Get all parts of the shape
    }
    
    void OnMouseEnter()
    {
        if (_shape == null || shapeComponents == null) return;

        foreach (GameObject part in shapeComponents)
        {
            if (part != null && part.TryGetComponent<Renderer>(out Renderer partRenderer))
            {
                partRenderer.material = _shape.HighlightMaterial; // Highlight all parts
                partRenderer.material.color = Color.cyan;
            }
        }
    }

    void OnMouseExit()
    {
        if (_shape == null || shapeComponents == null) return;

        foreach (GameObject part in shapeComponents)
        {
            if (part != null && part.TryGetComponent<Renderer>(out Renderer partRenderer))
            {
                partRenderer.material = _shape.DefaultMaterial; // Restore original material
                partRenderer.material.color = Color.red;
            }
        }
    }
}
