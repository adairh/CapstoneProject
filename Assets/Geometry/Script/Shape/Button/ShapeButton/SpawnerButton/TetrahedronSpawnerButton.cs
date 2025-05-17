using UnityEngine;

namespace Manipulator
{
    public class TetrahedronSpawnerButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.TetrahedronSpawner;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log($"{GetShapeType()} Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}