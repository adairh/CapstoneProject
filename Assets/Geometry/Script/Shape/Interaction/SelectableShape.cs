using System;
using UnityEngine;

namespace Manipulator
{
    public class SelectableShape : ShapeBehaviourBase
    {
        private bool isSelected;
        private Renderer rend;

        private void Awake()
        {
            rend = GetComponentInChildren<Renderer>();
        }

        public override void SetShape(Shape s)
        {
            base.SetShape(s);
        }

        public void SetSelected(bool selected)
        {
            if (ManipulationManager.Instance.IsDrawing) return;
            if (isSelected == selected) return; // Don't spam

            isSelected = selected;
            if (rend != null)
                MaterialLibrary.Apply(rend, isSelected ? MaterialType.Select : shape is ShapeMesh ? MaterialType.Mesh : MaterialType.Default);

            OnSelectedChanged?.Invoke(this);

            // If selection changed, propagate to dependents just once
            foreach (var s in shape.GetDependentShapesForDelete())
            {
                var select = s.GetComponent<SelectableShape>();
                if (select != null)
                    select.SetSelected(selected);
            }
        }

        public bool IsSelected() => isSelected;

        public event Action<SelectableShape> OnSelectedChanged;
    }
}