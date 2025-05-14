using UnityEngine;

namespace Manipulator
{
    public class DraggableShape : ShapeBehaviourBase
    {
        private bool isDragging = false;
        private Vector3 offset;
        private Vector3 dragStartPosition;

        private void OnMouseDown()
        {
            if (shape == null || ManipulationManager.Instance.IsDrawing)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                dragStartPosition = shape.transform.position;

                Vector3 mouseWorld = hit.point;
                offset = shape.transform.position - mouseWorld;
            }
        }

        private void OnMouseDrag()
        {
            if (!isDragging || shape == null || ManipulationManager.Instance.IsDrawing)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 rawTarget = hit.point + offset;
                Vector3 current = shape.transform.position;

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    // Move only on XZ
                    rawTarget.y = current.y;
                }
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    // Move only on Y
                    rawTarget.x = current.x;
                    rawTarget.z = current.z;
                }
                // else: free movement

                shape.MoveTo(rawTarget, queue: false);
            }
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
