using UnityEngine;

public class NameSetting : Setting<string>
{
    public NameSetting(string name) : base(name, ISetting.SettingType.NONNUMERIC, typeof(Shape)) { }
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