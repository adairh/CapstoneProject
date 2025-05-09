using System;
using UnityEngine;

namespace Manipulator
{
    public class SelectableShape : ShapeBehaviourBase
    {
        private Renderer rend;
        private Material defaultMat;
        private Material selectedMat;
        private bool isSelected = false;

        private void Awake()
        {
            rend = GetComponentInChildren<Renderer>();
            defaultMat = MaterialLibrary.Get(MaterialType.Default);
            selectedMat = MaterialLibrary.Get(MaterialType.Select);
            SetSelected(false);
        }

        public override void SetShape(Shape s)
        {
            base.SetShape(s);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (rend != null)
            {
                rend.material = isSelected ? selectedMat : defaultMat;
                OnSelectedChanged?.Invoke(this);
            }
        }

        public bool IsSelected() => isSelected;
        
        public event Action<SelectableShape> OnSelectedChanged;

    }
}
