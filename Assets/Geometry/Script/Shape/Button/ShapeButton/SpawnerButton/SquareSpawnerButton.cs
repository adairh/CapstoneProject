using UnityEngine;

namespace Manipulator
{
    public class SquareSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.SquareSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}