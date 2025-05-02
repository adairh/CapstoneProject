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
        
        
        private void Start()
        {
            InputManager.Instance.OnAction += HandleAction;
            _panelSpawner = new SpawnPanel();
        }
        private void OnDestroy()
        {
            InputManager.Instance.OnAction -= HandleAction;
        }
        private Shape _shape;
        private SpawnPanel _panelSpawner;

        public void SetShape(Shape shape)
        {
            // climb up to the root shape
            _shape = shape;
            while (_shape.Parent != null)
                _shape = _shape.Parent;
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
            else if (action == UserAction.AngleCons)
            {
                TryApplyAngleConstraint();
            }
            else if (action == UserAction.Config)
            {
                SetShape(_shape);
                _panelSpawner.SpawnPanelAtTop(_shape);
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
            Debug.Log($"[StartDragging] currentDraggingObject null? {currentDraggingObject == null}");
            Debug.Log($"[StartDragging] DragState = {CurrentDragState}");
            Debug.Log($"[StartDragging] IsDrawing? {IsDrawing()}");

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
            // 1) Xác định tất cả target cần toggle
            var targets = new List<Shape>();

            if (shape.Parent != null)
            {
                // Nếu là child, chọn parent
                targets.Add(shape.Parent);
            }
            else if (shape is Point p && p.AttachedShapes.Count > 0)
            {
                // Nếu là Point và có nhiều shape gắn vào, chọn hết
                targets.AddRange(p.AttachedShapes);
            }
            else
            {
                // Không có parent hay attached shapes → chọn chính nó
                targets.Add(shape);
            }

            // 2) Với mỗi target, toggle và đổi màu
            foreach (var target in targets)
            {
                bool nowSelected = !selectedShapes.Contains(target);
                if (nowSelected)
                    selectedShapes.Add(target);
                else
                    selectedShapes.Remove(target);

                // Chọn material tương ứng
                var mat = MaterialLibrary.Get(nowSelected 
                    ? MaterialType.Select 
                    : MaterialType.Default);

                // Áp lên tất cả component của target
                foreach (var go in target.Components())
                {
                    if (go.TryGetComponent<Renderer>(out var rend))
                        rend.material = mat;
                }
            }
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
 

        
        // === Constraint
        
        // === Angle
        
        /// <summary>
        /// Tìm điểm chung (pivot) và hai đầu còn lại (freeA, freeB) của hai segment.
        /// </summary>
        /// <param name="segA">Segment A</param>
        /// <param name="segB">Segment B</param>
        /// <param name="pivot">(out) điểm chung</param>
        /// <param name="freeA">(out) đầu tự do thuộc segA</param>
        /// <param name="freeB">(out) đầu tự do thuộc segB</param>
        public static void GetSharedPivotPoints(
            Segment segA,
            Segment segB,
            out Point pivot,
            out Point freeA,
            out Point freeB)
        {
            if (segA.Start == segB.Start)
            {
                pivot = segA.Start;
                freeA = segA.End;
                freeB = segB.End;
            }
            else if (segA.Start == segB.End)
            {
                pivot = segA.Start;
                freeA = segA.End;
                freeB = segB.Start;
            }
            else if (segA.End == segB.Start)
            {
                pivot = segA.End;
                freeA = segA.Start;
                freeB = segB.End;
            }
            else if (segA.End == segB.End)
            {
                pivot = segA.End;
                freeA = segA.Start;
                freeB = segB.Start;
            }
            else
            {
                throw new InvalidOperationException("Hai segment không có điểm chung!");
            }
        }
        
        private Point pivot, freeA, freeB;
        
        private void TryApplyAngleConstraint()
        {
            // Muốn đúng 2 shape được chọn
            if (selectedShapes.Count == 2)
            {
                // Lọc chỉ lấy 2 Segment
                var segments = selectedShapes.OfType<Segment>().ToList();
                if (segments.Count == 2)
                {
                    var segA = segments[0];
                    var segB = segments[1];

                    // Tính góc hiện tại giữa 2 vector của 2 segment
                    float currentAngle = Vector3.Angle(
                        (segA.End.Position - segA.Start.Position),
                        (segB.End.Position - segB.Start.Position)
                    );
                    
                    GetSharedPivotPoints(segA, segB, out pivot, out freeA, out freeB);
                    
                    var ac = pivot.GO.AddComponent<AngleConstraint>();
                    pivot.AppendSettings(new AngleSetting(ac));
                    ac.Owner = pivot;
                    ac.AddDependencies(segA, segB, pivot, currentAngle);
                    
                    
                    // Tạo constraint và đăng ký luôn trong constructor
                    // var constraint = new AngleConstraint(segA, segB, currentAngle);

                    
                    
                    Debug.Log(
                        $"<color=green>[AngleConstraint]</color> " +
                        $"Áp dụng giữa {segA.Name} và {segB.Name} với góc ban đầu " +
                        $"<color=green>{currentAngle:F1}°</color>"
                    );
                }
                else
                {
                    Debug.Log("<color=red>[AngleConstraint]</color> Phải chọn chính xác 2 Segment!");
                }
            }
            else
            {
                Debug.Log("<color=yellow>[AngleConstraint]</color> Vui lòng chọn đúng 2 Shape để áp dụng.");
            }
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
