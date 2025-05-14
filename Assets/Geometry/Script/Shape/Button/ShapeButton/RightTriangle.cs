using UnityEngine;

namespace Manipulator
{
    public class RightTriangleButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.RightTriangle;

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}