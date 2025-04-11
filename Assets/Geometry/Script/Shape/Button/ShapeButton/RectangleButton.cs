using UnityEngine;

public class RectangleButton : BaseButton, IShapeButton
{
    public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.Rectangle;

    protected override void OnButtonClick()
    {
        base.OnButtonClick();
        Debug.Log("Rectangle Button Clicked!");
        ShapeButtonManager.SetActiveShape(GetShapeType());
    }
}