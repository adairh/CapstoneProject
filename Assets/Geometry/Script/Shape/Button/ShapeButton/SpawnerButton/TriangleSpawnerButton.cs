using UnityEngine;

namespace Manipulator
{
    public class TriangleSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.TriangleSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Triangle Button Clicked!");
            //ShapeButtonManager.SetActiveShape(GetShapeType());
            //ShapeInputController.Instance.SetSpawner(new TriangleSpawner());
        }
    }
}