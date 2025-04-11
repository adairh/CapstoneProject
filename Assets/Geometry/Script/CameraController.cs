using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float zoomSpeed = 10f;
    public float panSpeed = 10f;

    private Vector3 lastMousePosition;

    void Update()
    { 
        // Rotate camera
        if (Input.GetMouseButton(1)) // Right-click to rotate
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            transform.RotateAround(Vector3.zero, Vector3.up, rotX);
            transform.RotateAround(Vector3.zero, transform.right, rotY);
        }

        // Zoom in/out
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.forward * scroll * zoomSpeed, Space.Self);

        // Pan camera
        if (Input.GetMouseButton(2)) // Middle-click to pan
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            transform.Translate(-delta.x * panSpeed * Time.deltaTime, -delta.y * panSpeed * Time.deltaTime, 0);
        }

        lastMousePosition = Input.mousePosition;
    }
}