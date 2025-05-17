using UnityEngine;

namespace Manipulator
{
    public class PointButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.Point;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Circle Point Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}