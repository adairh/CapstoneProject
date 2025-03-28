using UnityEngine;

public class StraightLineButton : BaseButton, IShapeButton
{
    public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.StraightLine;

    protected override void OnButtonClick()
    {
        base.OnButtonClick();
        Debug.Log("StraightLine Button Clicked!");
        ShapeButtonManager.SetActiveShape(GetShapeType());
    }
}