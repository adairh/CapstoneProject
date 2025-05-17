using UnityEngine;

public class ScalableShape : MonoBehaviour
{
    private Vector3 initialMousePos;
    private Vector3 initialScale;
    private bool isScaling;

    private void OnMouseDown()
    {
        isScaling = true;
        initialMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        initialScale = transform.localScale;
    }

    private void OnMouseDrag()
    {
        if (!isScaling) return;

        var currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var scaleFactor = currentMousePos.x - initialMousePos.x + (currentMousePos.y - initialMousePos.y);

        // ✅ Ensure the shape does not shrink too much
        var newWidth = Mathf.Max(0.5f, initialScale.x + scaleFactor);
        var newHeight = Mathf.Max(0.5f, initialScale.y + scaleFactor);

        transform.localScale = new Vector3(newWidth, newHeight, transform.localScale.z);
    }

    private void OnMouseUp()
    {
        isScaling = false;
    }
}