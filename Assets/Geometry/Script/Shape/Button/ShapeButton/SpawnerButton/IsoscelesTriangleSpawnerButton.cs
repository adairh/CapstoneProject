using UnityEngine;

namespace Manipulator
{
    public class IsoscelesTriangleSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.IsoscelesTriangleSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            //ShapeButtonManager.SetActiveShape(GetShapeType());
            ShapeInputController.Instance.SetSpawner(new IsoscelesTriangleSpawner());

        }
    }
}