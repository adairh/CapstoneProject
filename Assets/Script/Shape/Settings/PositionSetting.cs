using UnityEngine;

public class PositionSetting : Setting<Vector3>
{
    public PositionSetting(Vector3 position) : base(position, ISetting.SettingType.NUMERIC, typeof(Shape)) { }
    public override GameObject GetUI()
    {
        //throw new System.NotImplementedException();
        return new GameObject();
    }

    public override void Apply(Shape obj)
    {
        //throw new System.NotImplementedException();
    }
    public override float Height()
    {
        return 0f;
    }
}