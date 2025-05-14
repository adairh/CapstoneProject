// Refactored PerformDrawing.cs to support drawing mode via enum selector

using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace Manipulator
{
    public class PerformDrawing : MonoBehaviour
    {
        public static PerformDrawing Instance { get; private set; }
 
 

        private enum DragState { None, Dragging }
        private DragState currentState = DragState.None;

        private string pendingStartPointId;
        private Point currentStartPoint;
        private Segment previewSegment;

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
                case IShapeButton.ShapeType.EquilateralPyramid:
                    PrebuiltDrawingHandler.Instance.StartDrawing(new EquilateralPyramidDrawer());
                    break; 
            }

        }

        public static bool RaycastMouse(out Vector3 hitPos)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hitPos = hit.point;
                return true;
            }

            hitPos = Vector3.zero;
            return false;
        }
        public static bool RaycastMouse(out Vector3 hitPos, out Shape shape)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 

            if (Physics.Raycast(ray, out RaycastHit hit))
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
    }
}
