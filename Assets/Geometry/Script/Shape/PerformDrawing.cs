using UnityEngine;
using Unity.Netcode;

namespace Manipulator
{
    public class PerformDrawing : NetworkBehaviour
    {
        public Camera mainCamera;

        private static IShapeButton.ShapeType currentShape = IShapeButton.ShapeType.None;

        void Start()
        {
            ShapeButtonManager.OnShapeChanged += HandleShapeChange;
            if (mainCamera == null) mainCamera = Camera.main;
        }

        void OnDestroy()
        {
            ShapeButtonManager.OnShapeChanged -= HandleShapeChange;
        }

        void HandleShapeChange(IShapeButton.ShapeType newShape)
        {
            currentShape = newShape;
        }

        void Update()
        {
            if (mainCamera == null || currentShape == IShapeButton.ShapeType.None) return;

            Vector3 mousePosition = Input.mousePosition;
            Vector3 screenPoint = mousePosition;

            // Convert mouse to world
            Vector3 hitPoint;
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            if (!TryGetWorldPoint(ray, out hitPoint)) return;

            switch (currentShape)
            {
                case IShapeButton.ShapeType.Circle:
                    Circle.Sketch(hitPoint, screenPoint, mainCamera);
                    break;
                case IShapeButton.ShapeType.Rectangle:
                    Rectangle.Sketch(hitPoint, screenPoint, mainCamera);
                    break;
                case IShapeButton.ShapeType.Triangle:
                    Triangle.Sketch(hitPoint, mainCamera);
                    break;
                case IShapeButton.ShapeType.Segment:
                    Segment.Sketch(hitPoint, mainCamera);
                    break;
                case IShapeButton.ShapeType.StraightLine:
                    StraightLine.Sketch(hitPoint, mainCamera);
                    break;
            }
        }

        bool TryGetWorldPoint(Ray ray, out Vector3 hitPoint)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hitPoint = hit.point;
                return true;
            }

            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter);
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }

        public static void ResetShape()
        {
            currentShape = IShapeButton.ShapeType.None;
            ShapeButtonManager.SetActiveShape(IShapeButton.ShapeType.None);
        }
    }
}
