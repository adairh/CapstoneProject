using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshPro for better UI

public class RadiusSetting : Setting<float>
{
    public RadiusSetting(float radius) : base(radius, ISetting.SettingType.NUMERIC, typeof(CircularShape)) { }
    
    public override GameObject GetUI()
    {
        // Create a parent object for the setting UI
        GameObject parentGO = new GameObject("RadiusSettingUI");
        RectTransform parentTransform = parentGO.AddComponent<RectTransform>();

        Debug.Log(Resources.LoadAll("Assets"));
        
        // Load Label Prefab
        GameObject labelPrefab = Resources.Load<GameObject>("UI/TextLabelPrefab");
        if (labelPrefab == null)
        {
            Debug.LogError("TextLabelPrefab not found in Resources/UI!");
            return null;
        }

        // Load Input Field Prefab
        GameObject inputPrefab = Resources.Load<GameObject>("UI/InputFieldPrefab");
        if (inputPrefab == null)
        {
            Debug.LogError("InputFieldPrefab not found in Resources/UI!");
            return null;
        }

        // Instantiate Label
        GameObject labelGO = Object.Instantiate(labelPrefab, parentTransform);
        TextMeshProUGUI labelText = labelGO.GetComponent<TextMeshProUGUI>();
        if (labelText != null) labelText.text = "Radius:";

        // Instantiate Input Field
        GameObject inputGO = Object.Instantiate(inputPrefab, parentTransform);
        TMP_InputField inputField = inputGO.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.text = Value.ToString();
            inputField.onEndEdit.AddListener(value =>
            {
                if (float.TryParse(value, out float result) && result > 0) // Ensure positive radius
                {
                    SetValue(result);
                }
                else
                {
                    inputField.text = Value.ToString(); // Reset if invalid
                }
            });
        }

        return parentGO; // ✅ Return the grouped UI components
    }




    public override void Apply(Shape obj)
    {
        if (obj is Sphere)
        {
            ((Sphere)obj).Radius = Value;
        }
    }
}