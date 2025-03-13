using UnityEngine;

// Base Interface for All Settings
public interface ISetting
{
    enum SettingType
    {
        NUMERIC,
        NONNUMERIC
    }

    object GetValue();  // Returns value as an object
    GameObject GetUI();
    SettingType Type { get; }
    
    void Apply();
    void Update();
    void SetValue(object value);
    float Height();
}

// Generic Abstract Class for Settings
public abstract class Setting<T> : ISetting
{
    public T Value { get; protected set; }
    
    public Shape targetShape { get; set; }
    public GameObject uiInstance { get; protected set; }
    public GameObject prefab { get; protected set; }
    
    
    public System.Type TargetShape { get; protected set; }
    public ISetting.SettingType Type { get; }  // Ensure it's correctly initialized

    protected Setting(T data, ISetting.SettingType settingType, System.Type targetShape)
    {
        Value = data;
        Type = settingType;
        TargetShape = targetShape;
    }

    public abstract GameObject GetUI();
    public abstract void Apply();
    public abstract float Height();
    public abstract void Update();

    public object GetValue() => Value;

    // Allow setting values from object (used for UI input)
    public void SetValue(object value)
    {
        if (value is T castValue)
        {
            SetValue(castValue);
        }
        else
        {
            Debug.LogError($"Invalid value type for setting {GetType().Name}. Expected {typeof(T)}, got {value.GetType()}.");
        }
    }

    // Virtual method allows customization in child classes
    public virtual void SetValue(T value)
    {
        //Debug.LogWarning(" set value = " + value + " - " + Value);
        Value = value;
    }
}