using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public static ButtonController Instance { get; private set; }

    private List<BaseButton> buttons = new List<BaseButton>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;  // Prevents further execution if a duplicate exists
        }
    }

    private void Start()
    {
        Debug.Log("ButtonController is initialized.");
    }

    public void RegisterButton(BaseButton button)
    {
        if (!buttons.Contains(button))
        {
            buttons.Add(button);
            Debug.Log($"Registered Button: {button.name}");
        }
    }

    public void OnButtonClicked(BaseButton button)
    {
        Debug.Log($"Button Clicked: {button.name}");
    }
}