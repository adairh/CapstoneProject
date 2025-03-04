using UnityEngine;

public class ShapeClickHandler : MonoBehaviour
{
    private Shape shape; // Reference to the shape (if applicable)

    public void SetShape(Shape shape)
    {
        this.shape = shape;
    }

    private void OnMouseEnter()
    {
        Debug.Log($"[Mouse Enter] {gameObject.name}");
    }

    private void OnMouseExit()
    {
        Debug.Log($"[Mouse Exit] {gameObject.name}");
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
            Debug.Log($"[Left Click Down] {gameObject.name}");
        if (Input.GetMouseButtonDown(1)) // Right-click
            Debug.Log($"[Right Click Down] {gameObject.name}");
        if (Input.GetMouseButtonDown(2)) // Middle-click
            Debug.Log($"[Middle Click Down] {gameObject.name}");
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0)) // Left-click
            Debug.Log($"[Left Click Up] {gameObject.name}");
        if (Input.GetMouseButtonUp(1))
        {
            // Right-click
            Debug.Log($"[Right Click Up] {gameObject.name}");
        }

        if (Input.GetMouseButtonUp(2)) // Middle-click
            Debug.Log($"[Middle Click Up] {gameObject.name}");
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
            Debug.Log($"[Left Click Held] {gameObject.name}");
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log($"[Right Click Held] {gameObject.name}");
            shape?.OpenConfigPanel();
        }

        if (Input.GetMouseButtonDown(2))
            Debug.Log($"[Middle Click Held] {gameObject.name}");
    }
}