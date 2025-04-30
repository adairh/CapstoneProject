using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manipulator
{
    public class HoverableShape : MonoBehaviour
    {
        private Renderer _renderer;
        private Material _defaultMaterial;
        private Shape _shape;
        private bool isChild;
        private List<GameObject> shapeComponents = new List<GameObject>();

        public void SetShape(Shape shape)
        {
            _shape = shape; // Detect shape
            _renderer = GetComponent<Renderer>();

            if (_renderer != null)
            {
                _renderer.material = _shape.DefaultMaterial;
            }

            isChild = transform.parent != null; // Check if this is a child object

        }


        private void OnMouseEnter()
        {
            if (_shape == null) return;

            ManipulationManager.Instance.PinShape(_shape);

            ManipulationManager.Instance.RegisterHoveredObject(this); // Register this object in manager

            if (ManipulationManager.Instance.AllHoverMode && !isChild)
            {
                foreach (GameObject part in shapeComponents)
                {
                    if (part.TryGetComponent<Renderer>(out Renderer partRenderer))
                    {
                        partRenderer.material = MaterialLibrary.Get(MaterialType.Hover);
                    }
                }
            }
            else
            {
                if (_renderer != null)
                {
                    _renderer.material = MaterialLibrary.Get(MaterialType.Hover);

                }
            }
        }

        private void OnMouseExit()
        {
            ManipulationManager.Instance.UnpinShape();
            ManipulationManager.Instance.ResetAllHoveredObjects(); // Reset everything when exiting
        }

        public void ResetHover()
        {
            if (_shape == null) return;

            // 1) Xác định xem shape (hoặc parent) có đang được select không
            bool isSelected = ManipulationManager.Instance.IsShapeOrParentSelected(_shape);

            // 2) Chọn material tương ứng chỉ trong 1 dòng
            var targetMat = MaterialLibrary.Get(
                isSelected 
                    ? MaterialType.Select 
                    : MaterialType.Default
            );

            if (ManipulationManager.Instance.AllHoverMode && !isChild)
            {
                foreach (GameObject part in shapeComponents)
                    part.GetComponent<Renderer>().material = targetMat;
            }
            else
            {
                _renderer.material = targetMat;
            }
        }

        public void SetComponents()
        {
            shapeComponents = new List<GameObject>(_shape.Components()); // Get all parts of the shape
        }
    }
}