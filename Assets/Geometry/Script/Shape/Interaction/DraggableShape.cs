using UnityEngine;

namespace Manipulator
{
    public class DraggableShape : ShapeBehaviourBase
    {
        private Vector3 dragStartPosition;
        private bool isDragging;
        private Vector3 lastMousePosition;
        private Vector3 offset;

        private void OnMouseDown()
        {
            Debug.Log($"[DraggableShape] OnMouseDown called on {gameObject.name}");

            if (shape == null)
            {
                Debug.LogWarning("[DraggableShape] Shape is null on mouse down.");
                return;
            }
            if (ManipulationManager.Instance.IsDrawing)
            {
                Debug.Log("[DraggableShape] ManipulationManager is currently drawing. Drag cancelled.");
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log($"[DraggableShape] Raycast from mouse pos {Input.mousePosition}");

            if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                dragStartPosition = shape.transform.position;
                offset = shape.transform.position - hit.point;
                lastMousePosition = Input.mousePosition;

                Debug.Log($"[DraggableShape] Drag started on {gameObject.name}. " +
                          $"StartPos: {dragStartPosition}, Offset: {offset}");
            }
            else
            {
                Debug.Log($"[DraggableShape] Raycast did not hit {gameObject.name}. No drag started.");
            }
        }

        private void OnMouseDrag()
        {
            if (!isDragging)
            {
                // Optional: comment out if too verbose
                // Debug.Log("[DraggableShape] Not dragging.");
                return;
            }
            if (shape == null)
            {
                Debug.LogWarning("[DraggableShape] Shape is null during drag.");
                return;
            }
            if (ManipulationManager.Instance.IsDrawing)
            {
                Debug.Log("[DraggableShape] ManipulationManager is drawing during drag. Drag cancelled.");
                return;
            }

            var mouseDelta = Input.mousePosition - lastMousePosition;
            Debug.Log($"[DraggableShape] Mouse drag detected. MouseDelta: {mouseDelta}");
            lastMousePosition = Input.mousePosition;

            // Convert delta from screen to world space
            float camDistance = Camera.main.WorldToScreenPoint(shape.transform.position).z;
            var worldPointNow = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camDistance));
            var worldPointPrev = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x - mouseDelta.x, Input.mousePosition.y - mouseDelta.y, camDistance));
            var worldDelta = worldPointNow - worldPointPrev;

            Debug.Log($"[DraggableShape] WorldDelta: {worldDelta}");

            var currentPos = shape.transform.position;
            var target = currentPos + worldDelta;

            // Constrain movement
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                Debug.Log("[DraggableShape] CTRL held: Restricting drag to XZ plane.");
                target.y = currentPos.y;
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                Debug.Log("[DraggableShape] SHIFT held: Restricting drag to Y axis.");
                target.x = currentPos.x;
                target.z = currentPos.z;
            }

            Debug.Log($"[DraggableShape] Moving shape {shape.ShapeId} from {currentPos} to {target}");

            shape.MoveTo(target, queue: false);
        }

        private void OnMouseUp()
        {
            Debug.Log($"[DraggableShape] OnMouseUp called on {gameObject.name}");

            if (!isDragging)
            {
                Debug.Log("[DraggableShape] Not dragging on mouse up.");
                return;
            }
            if (shape == null)
            {
                Debug.LogWarning("[DraggableShape] Shape is null on mouse up.");
                return;
            }
            if (ManipulationManager.Instance.IsDrawing)
            {
                Debug.Log("[DraggableShape] ManipulationManager is drawing on mouse up. Drag cancelled.");
                return;
            }

            isDragging = false;
            var dragEndPosition = shape.transform.position;

            Debug.Log($"[DraggableShape] Drag ended. Start: {dragStartPosition}, End: {dragEndPosition}");

            if (dragEndPosition != dragStartPosition)
            {
                Debug.Log($"[DraggableShape] Broadcasting MoveShapeAction from {dragStartPosition} to {dragEndPosition} for shape {shape.ShapeId}");
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(
                    new MoveShapeAction(shape.ShapeId, dragStartPosition, dragEndPosition)
                );
            }
            else
            {
                Debug.Log("[DraggableShape] Drag ended with no position change. No action broadcasted.");
            }
        }
    }
}
