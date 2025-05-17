using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class ManipulationManager : MonoBehaviour
    {
        public static ManipulationManager Instance { get; private set; }

        public bool IsDrawing { get; set; }
        
        public Vector3 TrackingPoint { get; set; }


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

        private void HandleAction(UserAction action, Vector2 screenPos)
        {
            if (action == UserAction.Delete)
                if (SelectedShape().Count > 0)
                    foreach (var s in SelectedShape())
                        if (ShapeStorage.Contains(s.ShapeId))
                            UndoRedoManager.Instance.Do(
                                new DeleteShapeBatchAction(s.GetDependentShapesForDelete()));

            if (action == UserAction.LeftClick)
            {
                var ray = Camera.main.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out var hit))
                {
                    var shape = hit.collider.GetComponentInParent<Shape>();
                    if (shape != null)
                        foreach (var s in shape.GetDependentShapesForDelete())
                        {
                            var select = s.GetComponent<SelectableShape>();
                            if (select != null) select.SetSelected(!shape.GetComponent<SelectableShape>().IsSelected());
                        }
                }
            }
        }

        public List<Shape> SelectedShape()
        {
            List<Shape> ret = new();
            foreach (var s in ShapeStorage.GetAllShapes())
            {
                var select = s.GetComponent<SelectableShape>();
                if (select != null)
                    if (select.IsSelected())
                        ret.Add(s);
            }

            return ret;
        }

        public List<Shape> GetPinnedShapes()
        {
            return SelectedShape();
        }
    }
}