using UnityEngine;

public class ScalableShape : MonoBehaviour
{
    private Vector3 initialScale;
    private Vector3 initialMousePos;
    private bool isScaling = false;

    void OnMouseDown()
    {
        isScaling = true;
        initialMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        initialScale = transform.localScale;
    }

    void OnMouseDrag()
    {
        if (!isScaling) return;

        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float scaleFactor = (currentMousePos.x - initialMousePos.x) + (currentMousePos.y - initialMousePos.y);

        // ✅ Ensure the shape does not shrink too much
        float newWidth = Mathf.Max(0.5f, initialScale.x + scaleFactor);
        float newHeight = Mathf.Max(0.5f, initialScale.y + scaleFactor);

        transform.localScale = new Vector3(newWidth, newHeight, transform.localScale.z);
    }

    void OnMouseUp()
    {
        isScaling = false;
    }
}