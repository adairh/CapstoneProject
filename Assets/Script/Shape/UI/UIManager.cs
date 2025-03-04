using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject settingsPanel; // Main UI panel
    [SerializeField] private Transform settingsContent; // Container for UI elements
    [SerializeField] private Button closeButton; // Close button
    
    private Queue<GameObject> uiPool = new Queue<GameObject>(); // Object pool for reuse
    private List<GameObject> activeUIElements = new List<GameObject>(); // Currently active UI elements

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        settingsPanel.SetActive(false);
        closeButton.onClick.AddListener(CloseConfigPanel);
    }

    /// <summary>
    /// Opens a UI panel for any selected object, dynamically generating UI components.
    /// </summary>
    public void ShowCustomPanel(Dictionary<string, object> uiElements)
    {
        settingsPanel.SetActive(true);
        GenerateCustomUI(uiElements);
    }

    /// <summary>
    /// Generates a fully dynamic UI panel based on requested elements.
    /// </summary>
    private void GenerateCustomUI(Dictionary<string, object> uiElements)
    {
        ClearPreviousUI();

        foreach (var entry in uiElements)
        {
            string elementType = entry.Key;
            object value = entry.Value;

            GameObject uiElement = CreateUIElement(elementType, value);
            if (uiElement != null)
            {
                uiElement.transform.SetParent(settingsContent, false);
                activeUIElements.Add(uiElement);
            }
            else
            {
                Debug.LogWarning($"Unsupported UI Component: {elementType}");
            }
        }
    }

    /// <summary>
    /// Creates a built-in UI component based on the type requested.
    /// </summary>
    private GameObject CreateUIElement(string type, object value)
    {
        GameObject uiElement = new GameObject(type);
        uiElement.AddComponent<RectTransform>();

        switch (type)
        {
            case "Text":
                Text text = uiElement.AddComponent<Text>();
                text.text = value.ToString();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.color = Color.black;
                text.fontSize = 18;
                break;

            case "Button":
                Button button = uiElement.AddComponent<Button>();
                uiElement.AddComponent<Image>().color = Color.gray;
                button.onClick.AddListener(() => Debug.Log("Button Clicked"));
                AddTextToButton(uiElement, value.ToString());
                break;

            case "Slider":
                Slider slider = uiElement.AddComponent<Slider>();
                slider.minValue = 0;
                slider.maxValue = 100;
                slider.value = (float)value;
                break;

            case "Toggle":
                Toggle toggle = uiElement.AddComponent<Toggle>();
                uiElement.AddComponent<Image>().color = Color.white;
                toggle.isOn = (bool)value;
                break;

            case "Dropdown":
                Dropdown dropdown = uiElement.AddComponent<Dropdown>();
                dropdown.options = new List<Dropdown.OptionData>
                {
                    new Dropdown.OptionData("Option 1"),
                    new Dropdown.OptionData("Option 2"),
                    new Dropdown.OptionData("Option 3")
                };
                dropdown.value = (int)value;
                break;

            case "InputField":
                InputField inputField = uiElement.AddComponent<InputField>();
                uiElement.AddComponent<Image>().color = Color.white;
                inputField.text = value.ToString();
                break;

            default:
                return null;
        }

        return uiElement;
    }

    /// <summary>
    /// Adds a Text label inside a Button.
    /// </summary>
    private void AddTextToButton(GameObject buttonObject, string textContent)
    {
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObject.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = textContent;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.color = Color.black;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.resizeTextForBestFit = true;
    }

    /// <summary>
    /// Clears previous UI elements and returns them to the pool.
    /// </summary>
    private void ClearPreviousUI()
    {
        foreach (var element in activeUIElements)
        {
            Destroy(element);
        }
        activeUIElements.Clear();
    }

    public void CloseConfigPanel()
    {
        settingsPanel.SetActive(false);
    }
}
