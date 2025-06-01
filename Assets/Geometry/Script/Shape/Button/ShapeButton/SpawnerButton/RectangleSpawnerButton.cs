using UnityEngine;

namespace Manipulator
{
    public class RectangleSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.RectangleSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Rectangle Button Clicked!");
            //ShapeButtonManager.SetActiveShape(GetShapeType());
            ShapeInputController.Instance.SetSpawner(new RectangleSpawner());

        }
    }
}