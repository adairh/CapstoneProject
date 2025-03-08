using UnityEngine;

public abstract class Setting<T> : ISetting
{
    public T Value { get; protected set; }
    public System.Type TargetShape { get; protected set; }

    public abstract GameObject GetUI();

    public ISetting.SettingType Type { get; }
    public abstract void Apply(Shape obj);
    public abstract float Height();

    protected Setting(T data, ISetting.SettingType settingType, System.Type targetShape)
    {
        Value = data;
        Type = settingType;
        TargetShape = targetShape;
    }


    public object GetValue() => Value;

    public void SetValue(T value)
    {
        Value = value;
    }

}
 

// Base Interface for All Settings
public interface ISetting
{
    // Move SettingType inside the interface
    enum SettingType
    {
        NUMERIC,
        NONNUMERIC
    }

    object GetValue();  // Returns value as an object
    GameObject GetUI();
    SettingType Type { get; }
    
    void Apply(Shape obj);

    float Height();
}
