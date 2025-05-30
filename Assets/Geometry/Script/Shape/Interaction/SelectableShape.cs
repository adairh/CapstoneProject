using System;
using UnityEngine;

namespace Manipulator
{
    public class SelectableShape : ShapeBehaviourBase
    {
        private bool isSelected;
        private Renderer rend;
        private Material selectedMat;

        private void Start()
        {
            rend = GetComponentInChildren<Renderer>();
            selectedMat = MaterialLibrary.Get(MaterialType.Select);
            //SetSelected(false);
        }

        private void Update()
        {
            foreach (var s in shape.GetDependentShapesForDelete())
            {
                var select = s.GetComponent<SelectableShape>();
                if (select != null) select.SetSelected(shape.GetComponent<SelectableShape>().IsSelected());
            }
        }

        public override void SetShape(Shape s)
        {
            base.SetShape(s);
        }

        public void SetSelected(bool selected)
        {
            if (ManipulationManager.Instance.IsDrawing) return;
            isSelected = selected;
            if (rend != null)
                MaterialLibrary.Apply(rend, isSelected ? MaterialType.Select : MaterialType.Default);
            OnSelectedChanged?.Invoke(this);
        }



        public bool IsSelected()
        {
            return isSelected;
        }

        public event Action<SelectableShape> OnSelectedChanged;
    }
}