using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.EquilateralTriangleSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("EquilateralTriangle Button Clicked!");
            //ShapeButtonManager.SetActiveShape(GetShapeType());
            ShapeInputController.Instance.SetSpawner(new EquilateralTriangleSpawner());
        }
    }
}