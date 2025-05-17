using UnityEngine;

namespace Manipulator
{
    public class PolygonButton : BaseButton, IShapeButton
    {
        public IShapeButton.ShapeType GetShapeType()
        {
            return IShapeButton.ShapeType.Polygon;
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            Debug.Log("Polygon Button Clicked!");
            ShapeButtonManager.SetActiveShape(GetShapeType());
        }
    }
}