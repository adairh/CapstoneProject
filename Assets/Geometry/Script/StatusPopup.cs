using System.Collections;
using TMPro;
using UnityEngine;

public class StatusPopup : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    private float displayTime = 2f;
    private float fadeTime = 1f;
    private CanvasGroup canvasGroup;

    private void EnsureCanvasGroupInitialized()
    {
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
        }
    }

    public void SetStatus(string message)
    {
        EnsureCanvasGroupInitialized();

        if (statusText != null)
        {
            statusText.text = message;
            Debug.Log($"Status set with message: {message}");
        }
        else
        {
            Debug.LogError("StatusText is not assigned in StatusPopup!");
        }

        Invoke(nameof(StartFade), displayTime);
        Debug.Log($"Scheduled fade-out for message: {message} after {displayTime} seconds");
    }

    private void StartFade()
    {
        Debug.Log("Starting fade-out process");
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("FadeOut coroutine started");

        EnsureCanvasGroupInitialized();

        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        Destroy(gameObject);
        Debug.Log("Status popup destroyed");
    }
}