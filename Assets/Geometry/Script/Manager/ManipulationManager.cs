using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public enum AxisLockMode { None, LockY, LockXZ }
    public class ManipulationManager : MonoBehaviour
    {
        
        
        [Header("Materials")]
        public Material universalMat;
        public Material meshMat;
        
        public static ManipulationManager Instance { get; private set; }

        public AxisLockMode CurrentAxisLock = AxisLockMode.None;
        
        public bool IsDrawing { get; set; }
        public Vector3 TrackingPoint { get; set; }

        private Shape clickedShape;
        private DraggableShape dragComponent;
        private float mouseDownTime;
        private Vector2 mouseDownScreenPos;
        private bool isDragging;

        private const float clickThresholdTime = 0.15f;
        private const float dragThresholdDistance = 10f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            InputManager.Instance.OnAction += HandleAction;
            IsDrawing = false;
        }

        private void OnDisable()
        {
            InputManager.Instance.OnAction -= HandleAction;
        }

        private void Update()
        {
            if (clickedShape != null && !isDragging)
            {
                if ((Time.time - mouseDownTime > clickThresholdTime) ||
                    (Vector2.Distance(Input.mousePosition, mouseDownScreenPos) > dragThresholdDistance))
                {
                    if (dragComponent != null)
                    {
                        dragComponent.BeginDrag();
                        isDragging = true;
                    }
                }
            }

            if (isDragging && dragComponent != null)
            {
                dragComponent.UpdateDrag();
            }
        }

        private void HandleAction(UserAction action, Vector2 screenPos)
        {
            switch (action)
            {
                case UserAction.Delete:
                    if (SelectedShape().Count > 0)
                        foreach (var s in SelectedShape())
                            if (ShapeStorage.Contains(s.ShapeId))
                                UndoRedoManager.Instance.Do(
                                    new DeleteShapeBatchAction(s.GetDependentShapesForDelete()));
                    break;

                case UserAction.Down:
                    var rayDown = Camera.main.ScreenPointToRay(screenPos);
                    if (Physics.Raycast(rayDown, out var hitDown))
                    {
                        clickedShape = hitDown.collider.GetComponentInParent<Shape>();
                        dragComponent = hitDown.collider.GetComponentInParent<DraggableShape>();

                        if (clickedShape != null)
                        {
                            mouseDownTime = Time.time;
                            mouseDownScreenPos = screenPos;
                            isDragging = false;
                        }
                    }
                    break;

                case UserAction.Up:
                    if (clickedShape != null)
                    {
                        if (!isDragging)
                        {
                            foreach (var s in clickedShape.GetDependentShapesForDelete())
                            {
                                var select = s.GetComponent<SelectableShape>();
                                if (select != null)
                                    select.SetSelected(!clickedShape.GetComponent<SelectableShape>().IsSelected());
                            }
                        }
                        else
                        {
                            dragComponent?.EndDrag();
                        }

                        clickedShape = null;
                        dragComponent = null;
                        isDragging = false;
                    }
                    break;
            }
        }

        public List<Shape> SelectedShape()
        {
            List<Shape> ret = new();
            foreach (var s in ShapeStorage.GetAllShapes())
            {
                var select = s.GetComponent<SelectableShape>();
                if (select != null && select.IsSelected())
                    ret.Add(s);
            }
            return ret;
        }

        public List<Shape> GetPinnedShapes() => SelectedShape();
    }
}
