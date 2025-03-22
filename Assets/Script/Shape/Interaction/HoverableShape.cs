using UnityEngine;

public class HoverableShape : MonoBehaviour
{
    private Renderer renderer;
    public Shape _shape;
    private GameObject[] shapeComponents; // Store all components of the shape

    public bool AllMode { get; set; }

    public void SetMaterials(Shape shape)
    {
        _shape = shape;

        AllMode = false;

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
        if (AllMode)
        {
            foreach (GameObject part in shapeComponents)
            {
                if (part != null && part.TryGetComponent<Renderer>(out Renderer partRenderer))
                {
                    partRenderer.material = _shape.HighlightMaterial; // Highlight all parts
                    partRenderer.material.color = Color.cyan;
                }
            }
        }
        else
        {
            if (_shape.Parent != null) return;
            
            if (_shape.GO != null && _shape.GO.TryGetComponent<Renderer>(out Renderer partRenderer))
            {
                partRenderer.material = _shape.HighlightMaterial; // Highlight all parts
                partRenderer.material.color = Color.cyan;
            }
        }
    }

    void OnMouseExit()
    {
        if (_shape == null || shapeComponents == null) return;
        if (AllMode)
        {
            foreach (GameObject part in shapeComponents)
            {
                if (part != null && part.TryGetComponent<Renderer>(out Renderer partRenderer))
                {
                    partRenderer.material = _shape.DefaultMaterial; // Restore original material
                    partRenderer.material.color = Color.red;
                }
            }
        }
        else
        {
            if (_shape.Parent != null) return;

            if (_shape.GO != null && _shape.GO.TryGetComponent<Renderer>(out Renderer partRenderer))
            {
                partRenderer.material = _shape.DefaultMaterial; // Highlight all parts
                partRenderer.material.color = Color.red;
            }
        }
    }
}
