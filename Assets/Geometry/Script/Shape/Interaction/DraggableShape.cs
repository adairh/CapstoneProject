using UnityEngine;

public class DraggableShape : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Shape _shape;
    private Color originalColor;
    private Renderer shapeRenderer;
    private Vector3 lastMousePosition;

    public void SetShape(Shape shape)
    {
        _shape = shape;
        shapeRenderer = _shape.GO.GetComponent<Renderer>();
        if (shapeRenderer != null)
        {
            originalColor = shapeRenderer.material.color; // Store original color
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click to start dragging
        {
            TryStartDragging();
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            DragObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }
    }

    private void TryStartDragging()
    {
        if (DragManager.Instance.currentState == DragManager.DragState.None) return; // ✅ Prevent dragging if disabled

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            if (DragManager.Instance.StartDragging(this)) // ✅ Only start dragging if allowed
            {
                offset = _shape.Position - hit.point;
                isDragging = true;
                lastMousePosition = Input.mousePosition; // ✅ Store initial mouse position

                // ✅ Change color to green while dragging
                if (shapeRenderer != null)
                {
                    shapeRenderer.material.color = Color.green;
                }
            }
        }
    }

    private void DragObject()
    {
        Vector3 allowedAxis = DragManager.Instance.GetAllowedAxis();
        
        // Convert mouse delta movement into world movement
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
        Vector3 worldDelta = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(_shape.Position).z)) - 
                             Camera.main.ScreenToWorldPoint(new Vector3(lastMousePosition.x, lastMousePosition.y, Camera.main.WorldToScreenPoint(_shape.Position).z));

        lastMousePosition = Input.mousePosition; // Update last mouse position

        // Apply movement along allowed axis only
        Vector3 newPosition = _shape.Position + Vector3.Scale(worldDelta, allowedAxis);
        _shape.MoveToPosition(newPosition);
    }

    private void StopDragging()
    {
        if (!isDragging) return;

        isDragging = false;
        DragManager.Instance.StopDragging(this); // ✅ Notify DragManager

        // ✅ Restore original color
        if (shapeRenderer != null)
        {
            shapeRenderer.material.color = originalColor;
        }
    }
}
