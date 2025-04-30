using System.Linq;
using UnityEngine;

namespace Manipulator
{
    public class DraggableShape : MonoBehaviour
    {
        private bool isDragging = false;
        private Plane dragPlane;
        private Vector3 lastWorldPoint;

        private Shape _shape;
        private Renderer shapeRenderer;
        private Color originalColor;

        public void SetShape(Shape shape)
        {
            _shape = shape;
            shapeRenderer = _shape.GO.GetComponent<Renderer>();
            if (shapeRenderer != null)
            {
                originalColor = shapeRenderer.material.color;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                TryStartDragging();

            if (isDragging && Input.GetMouseButton(0))
                DragObject();

            if (Input.GetMouseButtonUp(0))
                StopDragging();
        }

        private void TryStartDragging()
        {
            //Debug.Log($"[Debug] TryStartDragging called. CurrentDragState = {ManipulationManager.Instance.CurrentDragState}");
            ManipulationManager mm = ManipulationManager.Instance;
            if (mm.CurrentDragState == ManipulationManager.DragState.None)
            {
                //Debug.Log("[Debug] CurrentDragState is None → return");
                return;
            }

            if (mm.IsDrawing())
            {
                //Debug.Log("[Debug] IsDrawing() == true → still sketching, cannot drag");
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.Log($"[Debug] Ray from camera: origin={ray.origin}, dir={ray.direction}");

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log($"[Debug] Raycast hit: {hit.collider.gameObject.name}");
                if (hit.collider.gameObject == gameObject)
                {
                    //Debug.Log("[Debug] Hit matches this gameObject → attempt StartDragging");
                    if (mm.StartDragging(this))
                    {
                        //Debug.Log("[Debug] StartDragging(this) returned true → setting up dragPlane");
                        SetupDragPlane(ray);
                        if (dragPlane.Raycast(ray, out float enter))
                        {
                            lastWorldPoint = ray.GetPoint(enter);
                            isDragging = true;
                            //Debug.Log($"[Debug] dragPlane.Raycast succeeded at distance {enter}, lastWorldPoint = {lastWorldPoint}");

                            // Visual feedback
                            if (shapeRenderer != null)
                            {
                                shapeRenderer.material = MaterialLibrary.Get(MaterialType.Drag);

                                //Debug.Log("[Debug] Changed shapeRenderer color to green");
                            }
                        }
                        else
                        {
                            //Debug.Log("[Debug] dragPlane.Raycast returned false → cannot compute lastWorldPoint");
                        }
                    }
                    else
                    {
                        //Debug.Log("[Debug] StartDragging(this) returned false → cannot drag");
                    }
                }
                else
                {
                    //Debug.Log("[Debug] Raycast hit other object, not this one");
                }
            }
            else
            {
                //Debug.Log("[Debug] Raycast did not hit anything");
            }
        }


        private void SetupDragPlane(Ray ray)
        {
            Vector3 normal = Vector3.up;

            switch (ManipulationManager.Instance.CurrentDragState)
            {
                case ManipulationManager.DragState.XZ:
                    normal = Vector3.up;
                    break;
                case ManipulationManager.DragState.Y:
                    normal = Vector3.forward; // Vertical plane (side view)
                    break;
                // Add more cases if needed
            }

            dragPlane = new Plane(normal, _shape.Position);
        }

        private void DragObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!dragPlane.Raycast(ray, out float enter)) return;

            Vector3 currentPoint = ray.GetPoint(enter);
            Vector3 delta = currentPoint - lastWorldPoint;
            Vector3 axis = ManipulationManager.Instance.GetAllowedDragAxis();
            Vector3 move = Vector3.Scale(delta, axis);
            lastWorldPoint = currentPoint;

            var mgr = ManipulationManager.Instance;
            // Nếu shape này nằm trong selectedShapes, drag tất cả
            if (mgr.SelectedShapes.Contains(_shape))
            {
                foreach (var s in mgr.SelectedShapes)
                {
                    s.MoveToPosition(s.Position + move);
                }
            }
            else
            {
                // Chỉ drag riêng
                _shape.MoveToPosition(_shape.Position + move);
            }
        }


        private void StopDragging()
        {
            if (!isDragging) return;

            isDragging = false;
            ManipulationManager.Instance.StopDragging(this);

            // ✅ Restore color
            if (shapeRenderer != null)
                shapeRenderer.material = MaterialLibrary.Get(MaterialType.Default);

        }
    }
}
