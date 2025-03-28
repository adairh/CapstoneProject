using System;
using UnityEngine; 

public class EditableShape : MonoBehaviour
{
        
    public ISetting[] Settings { get; set; }

    void Start()
    {
        ISetting[] defaultSetting =
        {
            new ColorSetting(Color.red),
            new NameSetting(this.ToString())
        };


        Settings = defaultSetting;
    }
    
    
    public void ApplySettings()
    {
        foreach (ISetting setting in Settings) // ✅ Corrected loop
        {
            setting.Apply(); // ✅ Apply each setting to the GameObject
        }
    }
    
    
}