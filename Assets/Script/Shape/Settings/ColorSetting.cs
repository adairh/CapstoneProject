using UnityEngine;

public class ColorSetting : Setting<Color>
{
    public ColorSetting(Color color) : base(color, ISetting.SettingType.NONNUMERIC, typeof(Shape)) { }
    public override GameObject GetUI()
    {
        //throw new System.NotImplementedException();
        return new GameObject();

    }

    public override void Apply(Shape obj)
    {
        obj.ShapeColor = Value;
        var renderer = obj.EditableShape.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Value; // Apply color to material
        }
    }
    
    
    public override float Height()
    {
        return 0f;
    }
}