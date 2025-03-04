using UnityEngine;

public class HoverableShape : MonoBehaviour
{
    private Renderer renderer;
    public Material defaultMaterial;
    public Material highlightMaterial;

    public void SetMaterials(Material defaultMat, Material highlightMat)
    {
        defaultMaterial = defaultMat;
        highlightMaterial = highlightMat;
        
        renderer = GetComponent<Renderer>();
        if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>(); 
        renderer.material = defaultMaterial;
    }

    void OnMouseEnter()
    {
        if (renderer != null) renderer.material = highlightMaterial; // Change color on hover
    }

    void OnMouseExit()
    {
        if (renderer != null) renderer.material = defaultMaterial; // Revert color
    }
}