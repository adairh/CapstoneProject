using UnityEngine;

namespace Manipulator
{
    public class HoverableShape : ShapeBehaviourBase
    {
        private Renderer rend;
        private Material defaultMat;
        private Material hoverMat;

        private void Awake()
        {
            rend = GetComponentInChildren<Renderer>();
            defaultMat = MaterialLibrary.Get(MaterialType.Default);
            hoverMat = MaterialLibrary.Get(MaterialType.Hover);
        }
        
        

        private void OnMouseEnter()
        {
            if (ManipulationManager.Instance.IsDrawing) return;
            var ss = shape.GetComponent<SelectableShape>();
            if (ss != null)
                if (ss.IsSelected())
                    return;
            if (rend != null)
            {
                rend.material = hoverMat;
            }
        }

        private void OnMouseExit()
        {
            if (ManipulationManager.Instance.IsDrawing) return;
            if (rend != null)
            {
                var ss = shape.GetComponent<SelectableShape>();
                if (ss != null)
                    if (ss.IsSelected())
                    {
                        //MaterialLibrary.Get(MaterialType.Select);
                        return;
                    }
                rend.material = defaultMat;
            }
        }
    }
}