using UnityEngine; 

public class LengthSetting : Setting<float>
{
    public LengthSetting(float length) : base(length, ISetting.SettingType.NUMERIC, typeof(Segment)) { }
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