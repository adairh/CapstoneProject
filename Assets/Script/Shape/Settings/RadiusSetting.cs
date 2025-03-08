using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadiusSetting : Setting<float>
{
    private const float ELEMENT_HEIGHT = 40f; 
    private const float SPACING = 10f; 

    private Shape targetShape; // Reference to the shape

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

        GameObject uiInstance = Object.Instantiate(prefab);

        TMP_InputField inputField = uiInstance.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.text = Value.ToString();
            inputField.onEndEdit.AddListener(value =>
            {
                if (float.TryParse(value, out float result) && result > 0)
                {
                    SetValue(result);
                    Apply(targetShape); // 🔥 Apply changes in real-time!
                }
                else
                {
                    inputField.text = Value.ToString();
                }
            });
        }

        return uiInstance;
    }

    public override void Apply(Shape obj)
    {
        if (obj is Sphere sphere)
        {
            sphere.Radius = Value;
            Debug.Log($"Applied new radius: {Value}");
            sphere.Draw();
        } else if (obj is Circle circle)
        {
            circle.Radius = Value;
            Debug.Log($"Applied new radius: {Value}");
            circle.Draw();
        }
    }

    public override float Height()
    {
        return (ELEMENT_HEIGHT * 2) + SPACING;
    }
}