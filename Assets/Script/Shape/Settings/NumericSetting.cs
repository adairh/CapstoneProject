﻿using UnityEngine;

public class NumericSetting : Setting<float>
{
    public NumericSetting(float length) : base(length, ISetting.SettingType.NUMERIC, typeof(Shape)) { }
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