using UnityEngine;

public class NoneButton : BaseButton, IShapeButton
{
    public IShapeButton.ShapeType GetShapeType() => IShapeButton.ShapeType.None;

    protected override void OnButtonClick()
    {
        base.OnButtonClick();
        Debug.Log("None Button Clicked!");
        ShapeButtonManager.SetActiveShape(GetShapeType());
    }
}