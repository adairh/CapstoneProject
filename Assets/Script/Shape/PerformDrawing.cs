using UnityEngine;

public class PerformDrawing : MonoBehaviour
{
    public Camera mainCamera; // Assign the camera in the Inspector

    public enum Type
    {
        Circle,
        Rectangle
    }

    public Type type;

    private bool drawing = false;
    private Vector3 startPoint; 
    private Shape _shape;

    void Update()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (type == Type.Circle)
                HandleCircleDrawing(hit.point);
            else if (type == Type.Rectangle)
                HandleRectangleDrawing(hit.point);
        }
    }

    private void HandleCircleDrawing(Vector3 hitPoint)
    {
        if (Input.GetMouseButtonDown(0)) // Click to start
        {
            if (!drawing)
            {
                startPoint = hitPoint;
                _shape = new Circle(startPoint, 0);
                drawing = true;
            }
        }
        else if (drawing && Input.GetMouseButton(0)) // Hold to resize
        {
            if (_shape is Circle circle)
            {
                float newRadius = Vector3.Distance(startPoint, hitPoint);
                if (!Mathf.Approximately(circle.Radius, newRadius)) // Prevent redundant updates
                {
                    circle.Radius = newRadius;
                    circle.GO.transform.rotation = GetAlignedRotation(); // Rotate based on camera
                    circle.Draw();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0)) // Release to finalize
        {
            drawing = false;
        }
    }


    private void HandleRectangleDrawing(Vector3 hitPoint)
    {
        if (Input.GetMouseButtonDown(0)) // Click to start
        {
            if (!drawing)
            {
                startPoint = hitPoint;
                _shape = new Rectangle(startPoint, 0, 0);
                drawing = true;
            }
        }
        else if (drawing && Input.GetMouseButton(0)) // Hold to resize
        {
            Vector3 size = hitPoint - startPoint;
            if (_shape is Rectangle rect)
            {
                float newWidth = size.x;
                float newHeight = size.z;
            
                if (!Mathf.Approximately(rect.Width, newWidth) || !Mathf.Approximately(rect.Height, newHeight))
                {
                    rect.Width = newWidth;
                    rect.Height = newHeight;
                
                    Vector3 newPos = startPoint + new Vector3(size.x / 2, 0, size.z / 2);
                    rect.Position = newPos;
                
                    rect.GO.transform.rotation = GetAlignedRotation(); // Rotate based on camera
                    rect.Draw();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0)) // Release to finalize
        {
            drawing = false;
        }
    }

    private Quaternion GetAlignedRotation()
    {
        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0; // Remove vertical tilt to keep it on the XZ plane
        if (forward == Vector3.zero) forward = Vector3.forward; // Fallback

        return Quaternion.LookRotation(forward, Vector3.up);
    }

    
}
