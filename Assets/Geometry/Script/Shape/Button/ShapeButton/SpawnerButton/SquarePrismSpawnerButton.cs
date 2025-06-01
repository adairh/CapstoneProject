using UnityEngine;

namespace Manipulator
{
    public class SquarePrismSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.SquarePrismSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            //ShapeButtonManager.SetActiveShape(GetShapeType());
            ShapeInputController.Instance.SetSpawner(new SquarePrismSpawner());
        }
    }
}