using UnityEngine;
using UnityEngine.UI;

public class BaseButton : MonoBehaviour
{
    protected Button button;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError($"[BaseButton] Missing Button component on {gameObject.name}");
            return;
        }

        button.onClick.AddListener(OnButtonClick);
    }

    protected virtual void Start()
    {
        if (ButtonController.Instance == null)
        {
            Debug.LogError("[BaseButton] ButtonController instance is null!");
            return;
        }

        ButtonController.Instance.RegisterButton(this);
    }

    protected virtual void OnButtonClick()
    {
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.OnButtonClicked(this);
        }
        else
        {
            Debug.LogError("[BaseButton] ButtonController.Instance is NULL on click!");
        }
    }
}