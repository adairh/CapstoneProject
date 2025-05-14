using UnityEngine;

namespace Manipulator
{
    public class RegularTetrahedronButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.RegularTetrahedron;

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}