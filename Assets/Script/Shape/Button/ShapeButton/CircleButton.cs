using UnityEngine;

public class CircleButton : BaseButton, IShapeButton
{
    public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.Circle;

    protected override void OnButtonClick()
    {
        base.OnButtonClick();
        Debug.Log("Circle Button Clicked!");
        ShapeButtonManager.SetActiveShape(GetShapeType());
    }
}