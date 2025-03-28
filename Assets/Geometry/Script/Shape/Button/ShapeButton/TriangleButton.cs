using UnityEngine;

public class TriangleButton : BaseButton, IShapeButton
{
    public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.Triangle;

    protected override void OnButtonClick()
    {
        base.OnButtonClick();
        Debug.Log("Triangle Button Clicked!");
        ShapeButtonManager.SetActiveShape(GetShapeType());
    }
}