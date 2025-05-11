using UnityEngine;

namespace Manipulator
{
    public class LineButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.Line;

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Line Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}