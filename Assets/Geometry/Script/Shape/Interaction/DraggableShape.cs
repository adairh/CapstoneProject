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
            if (shape == null)
            {
                //Debug.LogWarning("[DraggableShape] OnMouseDown: shape is null");
                return;
            }

            if (ManipulationManager.Instance.IsDrawing)
            {
                //Debug.Log("[DraggableShape] OnMouseDown: currently drawing, cannot drag");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject != gameObject)
                {
                    //Debug.Log("[DraggableShape] OnMouseDown: hit wrong object");
                    return;
                }

                isDragging = true;
                dragStartPosition = shape.transform.position;

                Vector3 mouseWorld = hit.point;
                offset = shape.transform.position - mouseWorld;

                //Debug.Log($"[DraggableShape] Start dragging shape ID={shape.ShapeId} at {dragStartPosition}");
            }
            else
            {
                //Debug.LogWarning("[DraggableShape] OnMouseDown: raycast hit nothing");
            }
        }

        private void OnMouseDrag()
        {
            if (!isDragging || shape == null)
            {
                //Debug.Log("[DraggableShape] OnMouseDrag: skipping, dragging=false or shape=null");
                return;
            }

            if (ManipulationManager.Instance.IsDrawing)
            {
                //Debug.Log("[DraggableShape] OnMouseDrag: currently drawing, skipping drag");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPos = hit.point + offset;
                shape.MoveTo(targetPos);
                //Debug.Log($"[DraggableShape] Dragging shape ID={shape.ShapeId} to {targetPos}");
            }
            else
            {
                //Debug.LogWarning("[DraggableShape] OnMouseDrag: raycast hit nothing");
            }
        }

        private void OnMouseUp()
        {
            if (!isDragging || shape == null)
            {
                //Debug.Log("[DraggableShape] OnMouseUp: not dragging or shape is null");
                return;
            }

            isDragging = false;

            if (ManipulationManager.Instance.IsDrawing)
            {
                //Debug.Log("[DraggableShape] OnMouseUp: currently drawing, canceling drag end");
                return;
            }

            Vector3 dragEndPosition = shape.transform.position;

            if (dragEndPosition != dragStartPosition)
            {
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(
                    new MoveShapeAction(shape.ShapeId, dragStartPosition, dragEndPosition)
                );
                //Debug.Log($"[DraggableShape] Finished dragging shape ID={shape.ShapeId} from {dragStartPosition} to {dragEndPosition}");
            }
            else
            {
                //Debug.Log("[DraggableShape] Dragged but position unchanged");
            }
        }
    }
}
