using UnityEngine;

public class SizeSetting : Setting<Vector3>
{
    public SizeSetting(Vector3 size) : base(size, ISetting.SettingType.NUMERIC, typeof(Shape)) { }
    public override GameObject GetUI()
    {
        //throw new System.NotImplementedException();
        return new GameObject();

    }

    public override void Apply(Shape obj)
    {
        //throw new System.NotImplementedException();
    }
}