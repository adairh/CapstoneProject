using UnityEngine; 

namespace Manipulator
{
    public class ManipulationManager : MonoBehaviour
    {
        public static ManipulationManager Instance { get; private set; }
        
        public bool IsDrawing { get; set; }

        private Shape selectedShape;

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
            {
                if (selectedShape != null)
                {
                    UndoRedoManager.Instance.Do(new DeleteShapeAction(selectedShape));
                    selectedShape = null;
                }
            }

            if (action == UserAction.LeftClick)
            {
                Ray ray = Camera.main.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var shape = hit.collider.GetComponentInParent<Shape>();
                    if (shape != null)
                    {
                        selectedShape = shape;
                        var select = selectedShape.GetComponent<SelectableShape>();
                        if (select != null)
                        {
                            select.SetSelected(!select.IsSelected());
                        }
                    }
                }
                
            }
        }

        public Shape GetPinnedShape() => selectedShape;
    }
}