using UnityEngine;

namespace Manipulator
{
    public class NoneButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.None;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("None Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}