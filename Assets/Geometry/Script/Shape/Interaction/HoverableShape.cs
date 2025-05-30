using UnityEngine;

namespace Manipulator
{
    public class HoverableShape : ShapeBehaviourBase
    {
        private Material hoverMat;
        private Renderer rend;

        private void Start()
        {
            rend = GetComponentInChildren<Renderer>();
            hoverMat = MaterialLibrary.Get(MaterialType.Hover);
        }


        private void OnMouseEnter()
        {
            if (ManipulationManager.Instance.IsDrawing) return;
            var ss = shape.GetComponent<SelectableShape>();
            if (ss != null && ss.IsSelected()) return;
            if (rend != null) MaterialLibrary.Apply(rend, MaterialType.Hover);
        }

        private void OnMouseExit()
        {
            if (ManipulationManager.Instance.IsDrawing) return;
            var ss = shape.GetComponent<SelectableShape>();
            if (ss != null && ss.IsSelected()) return;
            if (rend != null) MaterialLibrary.Apply(rend, MaterialType.Default);
        }

    }
}