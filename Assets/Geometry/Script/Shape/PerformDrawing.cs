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

        public enum DrawingMode
        {
            None,
            Point,
            Segment
        }

        [Header("Drawing Mode")]
        public DrawingMode drawingMode = DrawingMode.Segment;

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

            switch (drawingMode)
            {
                case DrawingMode.Point:
                    Point.Drawer.UpdatePointInput();
                    break;
                case DrawingMode.Segment:
                    Segment.Drawer.UpdateSegmentInput();
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


        public static void ResetMode()
        {
            if (Instance != null)
                Instance.drawingMode = DrawingMode.None;
        }
    }
}
