using UnityEngine;

namespace Manipulator
{
    public class DraggableShape : MonoBehaviour
    {
        private bool isDragging = false;
        private Plane dragPlane;
        private Vector3 lastWorldPoint;

        private Shape _shape;
        private Renderer shapeRenderer;
        private Color originalColor;

        public void SetShape(Shape shape)
        {
            _shape = shape;
            shapeRenderer = _shape.GO.GetComponent<Renderer>();
            if (shapeRenderer != null)
            {
                originalColor = shapeRenderer.material.color;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                TryStartDragging();

            if (isDragging && Input.GetMouseButton(0))
                DragObject();

            if (Input.GetMouseButtonUp(0))
                StopDragging();
        }

        private void TryStartDragging()
        {
            ManipulationManager mm = ManipulationManager.Instance;
            if (mm.CurrentDragState == ManipulationManager.DragState.None)
                return;

            if (mm.IsDrawing())
            {
                Debug.Log($"Drawing {mm.IsDrawing()}");
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            { 
                if (mm.StartDragging(this))
                { 
                    SetupDragPlane(ray);
                    if (dragPlane.Raycast(ray, out float enter))
                    { 
                        lastWorldPoint = ray.GetPoint(enter);
                        isDragging = true;

                        // ✅ Visual Feedback
                        if (shapeRenderer != null)
                            shapeRenderer.material.color = Color.green;
                    }
                }
            }
        }

        private void SetupDragPlane(Ray ray)
        {
            Vector3 normal = Vector3.up;

            switch (ManipulationManager.Instance.CurrentDragState)
            {
                case ManipulationManager.DragState.XZ:
                    normal = Vector3.up;
                    break;
                case ManipulationManager.DragState.Y:
                    normal = Vector3.forward; // Vertical plane (side view)
                    break;
                // Add more cases if needed
            }

            dragPlane = new Plane(normal, _shape.Position);
        }

        private void DragObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 currentWorldPoint = ray.GetPoint(enter);
                Vector3 delta = currentWorldPoint - lastWorldPoint;

                Vector3 allowedAxis = ManipulationManager.Instance.GetAllowedDragAxis();
                Vector3 constrainedDelta = Vector3.Scale(delta, allowedAxis);

                _shape.MoveToPosition(_shape.Position + constrainedDelta);
                lastWorldPoint = currentWorldPoint;
            }
        }

        private void StopDragging()
        {
            if (!isDragging) return;

            isDragging = false;
            ManipulationManager.Instance.StopDragging(this);

            // ✅ Restore color
            if (shapeRenderer != null)
                shapeRenderer.material.color = originalColor;
        }
    }
}
