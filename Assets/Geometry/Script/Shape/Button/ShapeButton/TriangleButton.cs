using UnityEngine;

namespace Manipulator
{
    public class TriangleButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.Triangle;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Triangle Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}