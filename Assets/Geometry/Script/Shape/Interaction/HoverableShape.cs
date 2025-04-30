using System.Collections.Generic;
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

            if (ManipulationManager.Instance.AllHoverMode && !isChild)
            {
                foreach (GameObject part in shapeComponents)
                {
                    if (part.TryGetComponent<Renderer>(out Renderer partRenderer))
                    {
                        partRenderer.material = MaterialLibrary.Get(MaterialType.Default);

                    }
                }
            }
            else
            {
                if (_renderer != null)
                {
                    _renderer.material = MaterialLibrary.Get(MaterialType.Default);

                }
            }
        }

        public void SetComponents()
        {
            shapeComponents = new List<GameObject>(_shape.Components()); // Get all parts of the shape
        }
    }
}