using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class ManipulationManager : MonoBehaviour
    {
        public GameObject shapeNetworkPrefab;
        public static ManipulationManager Instance { get; private set; }

        // === Dragging ===
        public enum DragState
        {
            XZ,
            Y,
            None
        }

        public DragState CurrentDragState;
        private DraggableShape currentDraggingObject = null;

        // === Hovering ===
        public bool AllHoverMode = false;
        private HashSet<HoverableShape> hoveredObjects = new HashSet<HoverableShape>();
        private Shape pinnedShape = null;

        // === Drawing ===

        private bool drawing = false;
        
        // === Temp / Straight Mode ===
        public enum Straight
        {
            X,
            Y,
            Z
        }

        public Straight ModeStraight;

        // === Init ===
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        // === Drag Methods ===
        public bool StartDragging(DraggableShape shape)
        {
            if (currentDraggingObject == null && CurrentDragState != DragState.None && !IsDrawing())
            {
                currentDraggingObject = shape;
                return true;
            }
            return false;
        }

        public void StopDragging(DraggableShape shape)
        {
            if (currentDraggingObject == shape)
            {
                currentDraggingObject = null;
            }
        }

        public Vector3 GetAllowedDragAxis()
        {
            switch (CurrentDragState)
            {
                case DragState.XZ: return new Vector3(1, 0, 1);
                case DragState.Y: return new Vector3(0, 1, 0);
                default: return Vector3.zero;
            }
        }

        // === Hover Methods ===
        public void RegisterHoveredObject(HoverableShape obj)
        {
            hoveredObjects.Add(obj);
        }

        public void ResetAllHoveredObjects()
        {
            foreach (HoverableShape obj in hoveredObjects)
            {
                if (obj != null)
                    obj.ResetHover();
            }
            hoveredObjects.Clear();
        }

        public void PinShape(Shape shape)
        {
            pinnedShape = shape;
        }

        public void UnpinShape()
        {
            pinnedShape = null;
        }

        public Shape GetPinnedShape()
        {
            return pinnedShape;
        }
        
        
        // === Drawing Methods ===


        public void SetDrawing(bool toggle)
        {
            drawing = toggle;
        }

        public bool IsDrawing()
        {
            return drawing;
        }
        
        
    }
}
