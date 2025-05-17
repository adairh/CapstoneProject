using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
    public string key;

    // Use this for initialization
    private void Start()
    {
        if (LocalizationManager.Instance.GetIsReady() == false)
            return;
        Debug.Log("Trying to get key: " + key);

        var text = GetComponent<Text>();

        if (text == null)
        {
            Debug.LogWarning("Can't find a text property when assigning localization key ' " + key +
                             "' to GameObject " + name);
            return;
        }

        text.text = LocalizationManager.Instance.GetLocalizedValue(key);
    }
}