using UnityEngine;

public class RotatableShape : MonoBehaviour
{
    private Vector3 initialMousePos;
    private float initialRotation;
    private bool isRotating;

    private void OnMouseDown()
    {
        isRotating = true;
        initialMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        initialRotation = transform.eulerAngles.z;
    }

    private void OnMouseDrag()
    {
        if (!isRotating) return;

        var currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var angleChange = (currentMousePos.x - initialMousePos.x) * 5f; // Adjust sensitivity

        transform.rotation = Quaternion.Euler(0, 0, initialRotation + angleChange);
    }

    private void OnMouseUp()
    {
        isRotating = false;
    }
}