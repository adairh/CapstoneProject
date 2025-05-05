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
            if (rend != null)
                rend.material = hoverMat;
        }

        private void OnMouseExit()
        {
            if (rend != null)
                rend.material = defaultMat;
        }
    }
}