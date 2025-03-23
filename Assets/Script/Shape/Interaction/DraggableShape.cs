using UnityEngine;

public class DraggableShape : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Shape _shape;
    private Color originalColor;
    private Renderer shapeRenderer;

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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPosition = hit.point + offset;
            Vector3 newPosition = Vector3.Scale(targetPosition, allowedAxis) + 
                                  Vector3.Scale(_shape.GO.transform.position, Vector3.one - allowedAxis);
            _shape.AdjustToPosition(newPosition);
        }
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
