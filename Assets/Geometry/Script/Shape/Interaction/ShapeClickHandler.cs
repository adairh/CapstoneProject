using UnityEngine;

namespace Manipulator
{
    public class ShapeClickHandler : MonoBehaviour
    {
        private Shape _shape;
        private SpawnPanel _panelSpawner;

        public void SetShape(Shape shape)
        {
            // climb up to the root shape
            _shape = shape;
            while (_shape.Parent != null)
                _shape = _shape.Parent;
        }

        private void Start()
        {
            // instantiate your helper; it will cache the Canvas for you
            _panelSpawner = new SpawnPanel();
        }

        private void OnMouseDown()
        {
            // only on right-click, once
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log($"[ShapeClick] spawning panel for {_shape.Name}");
                _panelSpawner.SpawnPanelAtTop(_shape);
            }
        }
    }
}