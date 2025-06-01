// Refactored PerformDrawing.cs to support drawing mode via enum selector

using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class PerformDrawing : MonoBehaviour
    {
        private Point currentStartPoint;
        private DragState currentState = DragState.None;

        private string pendingStartPointId;
        private Segment previewSegment;
        public static PerformDrawing Instance { get; private set; }

        private void Awake()
        {
            Debug.Log("PerformDrawing Awake on: " + gameObject.name);
            Instance = this;
        }


        private void Update()
        {
            if (!NetworkManager.Singleton.IsHost) return;

            switch (ShapeButtonManager.ActiveType)
            {
                case IShapeButton.ShapeType.Point:
                    Point.Drawer.UpdatePointInput();
                    break;
                case IShapeButton.ShapeType.Segment:
                    Segment.Drawer.UpdateSegmentInput();
                    break;
                case IShapeButton.ShapeType.Line:
                    Line.Drawer.UpdateLineInput();
                    break;
                case IShapeButton.ShapeType.RayShape:
                    RayShape.Drawer.UpdateRayShapeInput();
                    break;
                case IShapeButton.ShapeType.Polygon:
                    Polygon.Drawer.UpdatePolygonInput();
                    break;

                // Prebuilt shapes
                case IShapeButton.ShapeType.EquilateralTriangle:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new EquilateralTriangleDrawer());
                    break;
                case IShapeButton.ShapeType.EquilateralPyramid:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new EquilateralPyramidDrawer());
                    break;
                case IShapeButton.ShapeType.IsoscelesTriangle:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new IsoscelesTriangleDrawer());
                    break;
                case IShapeButton.ShapeType.Square:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new SquareDrawer());
                    break;
                case IShapeButton.ShapeType.Rectangle:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new RectangleDrawer());
                    break;
                case IShapeButton.ShapeType.Rhombus:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new RhombusDrawer());
                    break;
                case IShapeButton.ShapeType.Tetrahedron:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new TetrahedronDrawer());
                    break;
                case IShapeButton.ShapeType.RegularTetrahedron:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new RegularTetrahedronDrawer());
                    break;
                case IShapeButton.ShapeType.GenericPyramid:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new GenericPyramidDrawer());
                    break;
                case IShapeButton.ShapeType.RightTriangle:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new RightTriangleDrawer());
                    break;
                case IShapeButton.ShapeType.SquarePrism:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new SquarePrismDrawer());
                    break;
                case IShapeButton.ShapeType.SquarePyramid:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new SquarePyramidDrawer());
                    break;
                
                /*case IShapeButton.ShapeType.EquilateralTriangleSpawner:
                    ShapeInputController.Instance.SetSpawner(new EquilateralTriangleSpawner());
                    break;
                case IShapeButton.ShapeType.EquilateralPyramidSpawner:
                    ShapeInputController.Instance.SetSpawner(new EquilateralPyramidSpawner());
                    break;
                case IShapeButton.ShapeType.IsoscelesTriangleSpawner:
                    ShapeInputController.Instance.SetSpawner(new IsoscelesTriangleSpawner());
                    break;
                case IShapeButton.ShapeType.SquareSpawner:
                    ShapeInputController.Instance.SetSpawner(new SquareSpawner());
                    break;
                case IShapeButton.ShapeType.RectangleSpawner:
                    ShapeInputController.Instance.SetSpawner(new RectangleSpawner());
                    break;
                case IShapeButton.ShapeType.RhombusSpawner:
                    ShapeInputController.Instance.SetSpawner(new RhombusSpawner());
                    break;
                case IShapeButton.ShapeType.TetrahedronSpawner:
                    ShapeInputController.Instance.SetSpawner(new TetrahedronSpawner());
                    break;
                case IShapeButton.ShapeType.RegularTetrahedronSpawner:
                    ShapeInputController.Instance.SetSpawner(new RegularTetrahedronSpawner());
                    break;
                case IShapeButton.ShapeType.GenericPyramidSpawner:
                    ShapeInputController.Instance.SetSpawner(new GenericPyramidSpawner());
                    break;
                case IShapeButton.ShapeType.RightTriangleSpawner:
                    ShapeInputController.Instance.SetSpawner(new RightTriangleSpawner());
                    break;
                case IShapeButton.ShapeType.SquarePrismSpawner:
                    ShapeInputController.Instance.SetSpawner(new SquarePrismSpawner());
                    break;
                case IShapeButton.ShapeType.SquarePyramidSpawner:
                    ShapeInputController.Instance.SetSpawner(new SquarePyramidSpawner());
                    break;
                case IShapeButton.ShapeType.SegmentSpawner:
                    ShapeInputController.Instance.SetSpawner(new SegmentSpawner());
                    break;*/
                     
            }
        }

        public static bool RaycastMouse(out Vector3 hitPos)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                hitPos = hit.point;
                return true;
            }

            hitPos = Vector3.zero;
            return false;
        }

        public static bool RaycastMouse(out Vector3 hitPos, out Shape shape)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                hitPos = hit.point;
                shape = hit.collider.GetComponentInParent<Shape>();
                return true;
            }

            hitPos = Vector3.zero;
            shape = null;
            return false;
        }


        public static void ResetMode()
        {
            ShapeButtonManager.SetActiveShape(IShapeButton.ShapeType.None);
        }


        private enum DragState
        {
            None,
            Dragging
        }
    }
}