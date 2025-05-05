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
            if (shape == null) return;
            isDragging = true;
            dragStartPosition = shape.transform.position;

            Vector3 mouseWorld = GetMouseWorld();
            offset = shape.transform.position - mouseWorld;
        }

        private void OnMouseDrag()
        {
            if (!isDragging || shape == null) return;

            Vector3 mouseWorld = GetMouseWorld();
            shape.MoveTo(mouseWorld + offset);
        }

        private void OnMouseUp()
        {
            if (!isDragging || shape == null) return;

            isDragging = false;
            Vector3 dragEndPosition = shape.transform.position;
            if (dragEndPosition != dragStartPosition)
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(new MoveShapeAction(shape.ShapeId, dragStartPosition, dragEndPosition));
        }

        private Vector3 GetMouseWorld()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                return hit.point;
            return shape.transform.position;
        }
    }
}