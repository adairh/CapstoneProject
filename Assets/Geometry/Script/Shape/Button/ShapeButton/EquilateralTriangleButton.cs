using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.EquilateralTriangle;

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("EquilateralTriangle Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}