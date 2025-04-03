using UnityEngine;

namespace Manipulator
{
    public class SegmentButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.Segment;

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Segment Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}