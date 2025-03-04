using UnityEngine;

public class AngleSetting : Setting<float>
{
    public AngleSetting(float angle) : base(angle, ISetting.SettingType.NUMERIC, typeof(Segment)) { }
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