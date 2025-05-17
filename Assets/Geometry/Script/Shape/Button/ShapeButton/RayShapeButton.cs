using UnityEngine;

namespace Manipulator
{
    public class RayShapeButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.RayShape;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Ray Shape Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}