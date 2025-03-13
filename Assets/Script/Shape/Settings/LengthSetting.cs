using UnityEngine; 

public class LengthSetting : Setting<float>
{
    public LengthSetting(float length) : base(length, ISetting.SettingType.NUMERIC, typeof(Segment)) { }
    public override GameObject GetUI()
    {
        //throw new System.NotImplementedException();
        return new GameObject();

    }

    public override void Apply()
    {
        //throw new System.NotImplementedException();
    }

    public override void Update()
    {
    }

    public override float Height()
    {
        return 0f;
    }
}