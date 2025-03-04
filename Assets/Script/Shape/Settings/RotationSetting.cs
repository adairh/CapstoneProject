using UnityEngine;

public class RotationSetting : Setting<Quaternion>
{
    public RotationSetting(Quaternion rotation) : base(rotation, ISetting.SettingType.NUMERIC, typeof(Shape)) { }
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