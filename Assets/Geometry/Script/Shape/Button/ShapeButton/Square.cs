using UnityEngine;

namespace Manipulator
{
    public class SquareButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.Square;

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}