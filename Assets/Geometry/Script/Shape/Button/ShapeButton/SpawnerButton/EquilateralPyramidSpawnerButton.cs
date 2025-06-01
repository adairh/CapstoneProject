using UnityEngine;

namespace Manipulator
{
    public class EquilateralPyramidSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.EquilateralPyramidSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            //ShapeButtonManager.SetActiveShape(GetShapeType());
            ShapeInputController.Instance.SetSpawner(new EquilateralPyramidSpawner());
        }
    }
}