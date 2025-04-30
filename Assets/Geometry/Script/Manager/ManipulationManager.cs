using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Manipulator
{
    public class ManipulationManager : MonoBehaviour
    {
        public GameObject shapeNetworkPrefab;
        public static ManipulationManager Instance { get; private set; }

        
        private float refreshTimer = 0f;
        private float refreshInterval = 1f;
 
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

        
        // === InputManager ===
        
        
        private void OnEnable()
        {
            InputManager.Instance.OnAction += HandleAction;
        }
        private void OnDisable()
        {
            InputManager.Instance.OnAction -= HandleAction;
        }
 
        private void HandleAction(UserAction action, Vector2 pos)
        {
            if (action == UserAction.LeftClick)
            {
                bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                Ray ray = Camera.main.ScreenPointToRay(pos);

                if (ctrl)
                {
                    // Việc chọn đã được xử lý trong ClickableShape.OnMouseDown.
                    // Không cần làm gì thêm nếu muốn giữ OnMouseDown.
                }
                else
                {
                    // Nếu click vùng trống (không hit collider nào), clear selection
                    if (!Physics.Raycast(ray, out _))
                    {
                        ClearSelection();
                    }
                }
            }

            // ... các case khác ...
        }
        
        
        // === Drag Methods ===
        public bool StartDragging(DraggableShape shape)
        {
            bool canDrag = currentDraggingObject == null 
                           && CurrentDragState != DragState.None 
                           && !IsDrawing();

            // In debug có màu: xanh nếu drag được, đỏ nếu không
            string color = canDrag ? "green" : "red";
            Debug.Log($"<color={color}>[StartDragging] Can drag {shape.name}? {canDrag}</color>");

            if (canDrag)
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
        
        
        
        // === Select ===
        
        public bool IsShapeOrParentSelected(Shape shape)
        {
            return selectedShapes.Contains(shape)
                   || (shape.Parent != null && selectedShapes.Contains(shape.Parent))
                   || (shape is Point p && p.AttachedShapes.Any(s => selectedShapes.Contains(s)));
        }

        
         private HashSet<Shape> selectedShapes = new HashSet<Shape>();
        public IReadOnlyCollection<Shape> SelectedShapes => selectedShapes;

        /// <summary>
        /// Toggle chọn / bỏ chọn một shape.
        /// </summary>
        public void ToggleSelection(Shape shape)
        {
            if (selectedShapes.Contains(shape))
            {
                // Bỏ chọn
                selectedShapes.Remove(shape);
            }
            else
            {
                // Thêm chọn
                selectedShapes.Add(shape);
            }

            // Update trực tiếp material
            shape.Components().ForEach(go =>
            {
                if (go.TryGetComponent<Renderer>(out var rend))
                    rend.material = selectedShapes.Contains(shape)
                        ? MaterialLibrary.Get(MaterialType.Select)
                        : MaterialLibrary.Get(MaterialType.Default);
            });
        }

        /// <summary>
        /// Clear hết selection khi ctrl không giữ và click vùng trống.
        /// </summary>
        public void ClearSelection()
        {
            foreach (var shape in selectedShapes)
            {
                shape.Components().ForEach(go =>
                {
                    if (go.TryGetComponent<Renderer>(out var rend))
                        rend.material = MaterialLibrary.Get(MaterialType.Default);
                });
            }
            selectedShapes.Clear();
        }
 



    }
    
    
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> src, System.Action<T> act)
        {
            foreach (var x in src) act(x);
        }
    }
    
    
}
