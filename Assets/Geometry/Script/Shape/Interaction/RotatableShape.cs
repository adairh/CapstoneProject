using UnityEngine;

public class RotatableShape : MonoBehaviour
{
    private bool isRotating = false;
    private Vector3 initialMousePos;
    private float initialRotation;

    void OnMouseDown()
    {
        isRotating = true;
        initialMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        initialRotation = transform.eulerAngles.z;
    }

    void OnMouseDrag()
    {
        if (!isRotating) return;

        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angleChange = (currentMousePos.x - initialMousePos.x) * 5f; // Adjust sensitivity

        transform.rotation = Quaternion.Euler(0, 0, initialRotation + angleChange);
    }

    void OnMouseUp()
    {
        isRotating = false;
    }
}