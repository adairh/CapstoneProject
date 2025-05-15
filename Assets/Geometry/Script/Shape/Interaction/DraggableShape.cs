using UnityEngine;

namespace Manipulator
{
    public class DraggableShape : ShapeBehaviourBase
    {
        private bool isDragging = false;
        private Vector3 offset;
        private Vector3 dragStartPosition;
        private Vector3 lastMousePosition;
        private void OnMouseDown()
        {
            if (shape == null || ManipulationManager.Instance.IsDrawing)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                dragStartPosition = shape.transform.position;

                offset = shape.transform.position - hit.point;

                lastMousePosition = Input.mousePosition;
            }
        }

        private void OnMouseDrag()
        {
            if (!isDragging || shape == null || ManipulationManager.Instance.IsDrawing)
                return;

            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // Chuyển delta từ screen space sang world space
            Vector3 worldDelta = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(shape.transform.position).z)) -
                                 Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x - mouseDelta.x, Input.mousePosition.y - mouseDelta.y, Camera.main.WorldToScreenPoint(shape.transform.position).z));

            Vector3 currentPos = shape.transform.position;
            Vector3 target = currentPos + worldDelta;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                // Only XZ
                target.y = currentPos.y;
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Only Y
                target.x = currentPos.x;
                target.z = currentPos.z;
            }

            shape.MoveTo(target, queue: false);
        }
        
        private void OnMouseUp()
        {
            if (!isDragging || shape == null || ManipulationManager.Instance.IsDrawing)
                return;

            isDragging = false;
            Vector3 dragEndPosition = shape.transform.position;

            if (dragEndPosition != dragStartPosition)
            {
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(
                    new MoveShapeAction(shape.ShapeId, dragStartPosition, dragEndPosition)
                );
            }
        }
    }
}
