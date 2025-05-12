using System;
using UnityEngine;

namespace Manipulator
{
    public class SelectableShape : ShapeBehaviourBase
    {
        private Renderer rend; 
        private Material selectedMat;
        private bool isSelected = false;

        private void Start()
        {
            rend = GetComponentInChildren<Renderer>();
            selectedMat = MaterialLibrary.Get(MaterialType.Select);
            //SetSelected(false);
        }

        public override void SetShape(Shape s)
        {
            base.SetShape(s);
        }

        private void Update()
        {
            foreach (var s in shape.GetDependentShapesForDelete())
            {
                var select = s.GetComponent<SelectableShape>();
                if (select != null)
                {
                    select.SetSelected(shape.GetComponent<SelectableShape>().IsSelected());
                }
            }
        }

        public void SetSelected(bool selected)
        {
            if (ManipulationManager.Instance.IsDrawing) return;
            isSelected = selected;
            if (rend != null)
            {
                rend.material = isSelected ? selectedMat : shape.DefaultMat;
                OnSelectedChanged?.Invoke(this);
            }
        }

        public bool IsSelected() => isSelected;
        
        public event Action<SelectableShape> OnSelectedChanged;

    }
}
