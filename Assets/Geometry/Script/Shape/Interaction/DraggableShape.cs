using UnityEngine;

namespace Manipulator
{
    public class DraggableShape : ShapeBehaviourBase
    {
        private Vector3 dragStartPosition;
        private bool isDragging;
        private Vector3 lastMousePosition;

        public void BeginDrag()
        {
            if (shape == null || ManipulationManager.Instance.IsDrawing)
                return;

            isDragging = true;
            dragStartPosition = shape.transform.position;
            lastMousePosition = Input.mousePosition;
        }

        public void UpdateDrag()
        {
            if (!isDragging || shape == null || ManipulationManager.Instance.IsDrawing)
                return;

            // --- NEW: Restrict point drag if constrained ---
            if (shape is Point point && point.TryGetComponent<RelativePointConstraint>(out var relConstraint) && relConstraint.enabled && relConstraint.TargetSegment != null)
            {
                var seg = relConstraint.TargetSegment;
                var a = seg.StartPoint.transform.position;
                var b = seg.EndPoint.transform.position;
                var ab = b - a;

                float camDistance = Camera.main.WorldToScreenPoint(point.transform.position).z;
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camDistance));
                float t = Mathf.Clamp01(Vector3.Dot(mouseWorld - a, ab.normalized) / ab.magnitude);

                relConstraint.T = t; // updates position via constraint
                lastMousePosition = Input.mousePosition;
                return;
            }


            // --- Default drag logic for unconstrained shapes or non-Points ---
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            float camDistanceDefault = Camera.main.WorldToScreenPoint(shape.transform.position).z;
            Vector3 worldPointNow = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camDistanceDefault));
            Vector3 worldPointPrev = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x - mouseDelta.x, Input.mousePosition.y - mouseDelta.y, camDistanceDefault));
            Vector3 worldDelta = worldPointNow - worldPointPrev;

            Vector3 currentPos = shape.transform.position;
            Vector3 target = currentPos + worldDelta;

            var lockMode = ManipulationManager.Instance.CurrentAxisLock;
            switch (lockMode)
            {
                case AxisLockMode.LockY:
                    target.y = currentPos.y; // Lock Y
                    break;
                case AxisLockMode.LockXZ:
                    target.x = currentPos.x;
                    target.z = currentPos.z; // Lock XZ
                    break;
                // Add more as needed
            }


            shape.MoveTo(target, queue: false);
        }

        public void EndDrag()
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
