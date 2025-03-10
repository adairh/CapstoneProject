using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadiusSetting : Setting<float>
{
    private const float ELEMENT_HEIGHT = 40f; 
    private const float SPACING = 10f; 

    private Shape targetShape; // Reference to the shape

    private GameObject uiInstance;

    public RadiusSetting(float radius, Shape shape) 
        : base(radius, ISetting.SettingType.NUMERIC, typeof(CircularShape))
    {
        targetShape = shape; 
    }

    public override GameObject GetUI()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager instance not found in the scene!");
            return null;
        }

        GameObject prefab = UIManager.Instance.GetUIComponent("RadiusSettingPrefab");

        if (prefab == null)
        {
            Debug.LogError("RadiusSettingPrefab not found in UIManager!");
            return null;
        }

        uiInstance = Object.Instantiate(prefab);


        Debug.Log(Value);
        
        
        TMP_InputField inputField = uiInstance.GetComponentInChildren<TMP_InputField>();
         

        if (inputField != null)
        {
            inputField.text = Value + "";
            inputField.onEndEdit.AddListener(value =>
            {
                if (float.TryParse(value, out float result) && result > 0)
                {
                    SetValue(result); 
                    Apply(targetShape); // 🔥 Apply changes in real-time!
                }
                else
                {
                    inputField.text = Value + "";
                }
            });
        }
        return uiInstance;
    }

    public override void Apply(Shape obj)
    {
        obj.ModifySetting(this, Value);
        obj.Draw();
    }


    public override float Height()
    {
        return (ELEMENT_HEIGHT * 2) + SPACING;
    }
}