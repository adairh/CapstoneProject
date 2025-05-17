using UnityEngine;

namespace Manipulator
{
    public class ShapeClickHandler : MonoBehaviour
    {
        private SpawnPanel panelSpawner;
        private Shape shape;

        private void Start()
        {
            panelSpawner = new SpawnPanel();
        }

        private void Update()
        {
            // mỗi khung, nếu right-click
            if (Input.GetMouseButtonDown(1))
            {
                var cam = Camera.main;
                if (cam == null) return;

                var ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    // debug
                    Debug.Log($"[ShapeClickHandler] Raycast hit: {hit.collider.gameObject.name}");
                    if (hit.collider.gameObject == gameObject)
                    {
                        Debug.Log("[ShapeClickHandler] Right-click on shape!");
                        panelSpawner.SpawnPanelAtTop(shape);
                    }
                }
            }
        }

        public void SetShape(Shape shape)
        {
            // luôn lấy root shape (nếu có nested)
            this.shape = shape; /*
            while (this.shape.Parent != null)
                this.shape = this.shape.Parent;*/
        }
    }
}