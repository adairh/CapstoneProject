using TMPro;
using UnityEngine;

public class PositionSetting : Setting<Vector3>
{ 
    public PositionSetting(Vector3 position, Shape shape) : base(position, ISetting.SettingType.NUMERIC, typeof(Shape)) {
        targetShape = shape; 
        prefab = UIManager.Instance.GetUIComponent("PositionSettingPrefab");
    }
    public override GameObject GetUI()
    {
        uiInstance = Object.Instantiate(prefab);
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager instance not found in the scene!");
            return null;
        }
        
        TMP_InputField[] inputFields = uiInstance.GetComponentsInChildren<TMP_InputField>();

        if (inputFields.Length >= 3) // Ensure we have three input fields (X, Y, Z)
        {
            Vector3 tempValue = Value; // Store Value in a local variable

            // Set input fields to current values
            inputFields[0].text = tempValue.x + "";
            inputFields[1].text = tempValue.y + "";
            inputFields[2].text = tempValue.z + "";

            // Add listeners for each field
            inputFields[0].onEndEdit.AddListener(value =>
            {
                if (float.TryParse(value, out float result))
                {
                    tempValue.x = result; // Modify tempValue
                    Value = tempValue; // Assign back to Value
                    Apply();
                }
                inputFields[0].text = tempValue.x + "";
            });

            inputFields[1].onEndEdit.AddListener(value =>
            {
                if (float.TryParse(value, out float result))
                {
                    tempValue.y = result;
                    Value = tempValue;
                    Apply();
                }
                inputFields[1].text = tempValue.y + "";
            });

            inputFields[2].onEndEdit.AddListener(value =>
            {
                if (float.TryParse(value, out float result))
                {
                    tempValue.z = result;
                    Value = tempValue;
                    Apply();
                }
                inputFields[2].text = tempValue.z + "";
            });
        }


        return uiInstance;
    }

    public override void Apply()
    {
        targetShape.GO.transform.position = Value;
        targetShape.Position = Value;
        targetShape.ModifySetting(this, Value);
        targetShape.CompleteSettings();
        targetShape.Draw();
        targetShape.UpdateHitbox();
    }

    public override void Update()
    {
        Value = targetShape.GO.transform.position;
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