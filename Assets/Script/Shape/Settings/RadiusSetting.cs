using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadiusSetting : Setting<float>
{ 



    public RadiusSetting(float radius, Shape shape) 
        : base(radius, ISetting.SettingType.NUMERIC, typeof(CircularShape))
    {
        targetShape = shape; 
        prefab = UIManager.Instance.GetUIComponent("RadiusSettingPrefab");
    }

    public override GameObject GetUI()
    {
        uiInstance = Object.Instantiate(prefab);
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager instance not found in the scene!");
            return null;
        }

        TMP_InputField inputField = uiInstance.GetComponentInChildren<TMP_InputField>();
         

        if (inputField != null)
        {
            inputField.text = Value + "";
            inputField.onEndEdit.AddListener(value =>
            {
                if (float.TryParse(value, out float result) && result > 0)
                {
                    SetValue(result); 
                    Apply(); // 🔥 Apply changes in real-time!
                }
                else
                {
                    inputField.text = Value + "";
                }
            });
        }
        return uiInstance;
    }

    public override void Apply()
    {
        targetShape.ModifySetting(this, Value);
        ((CircularShape)targetShape).Radius = Value;
        targetShape.CompleteSettings();
        targetShape.Draw();
        targetShape.UpdateHitbox();
    }

    public override void Update()
    {
        Value = ((CircularShape)targetShape).Radius;
    }

    public override float Height()
    {
        if (prefab.TryGetComponent<RectTransform>(out var rectTransform))
        {
            return rectTransform.rect.height; // Get height of UI panel
        }
        return 0f; // Default if no RectTransform is found
    }


}