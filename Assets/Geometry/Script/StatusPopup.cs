using System.Collections;
using TMPro;
using UnityEngine;

public class StatusPopup : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public float displayTime = 2f;
    public float fadeTime = 1f;
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
        // Ensure the GameObject is active before proceeding
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        EnsureCanvasGroupInitialized();

        if (statusText != null)
        {
            statusText.text = message;
            Debug.Log($"Status set with message: {message}");
        }
        else
        {
            Debug.LogError("StatusText is not assigned in StatusPopup!");
            return;
        }

        // Cancel any existing invoke to prevent multiple fade starts
        CancelInvoke(nameof(StartFade));
        Invoke(nameof(StartFade), displayTime);
        Debug.Log($"Scheduled fade-out for message: {message} after {displayTime} seconds");
    }

    private void StartFade()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("FadeOut coroutine started");

        EnsureCanvasGroupInitialized();

        float elapsedTime = 0f;
        while (elapsedTime < fadeTime && gameObject.activeSelf)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            yield return null;
        }

        // Ensure we're fully faded out
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        // Only destroy if we're still active (in case the object was deactivated during fade)
        if (gameObject.activeSelf)
        {
            Destroy(gameObject);
            Debug.Log("Status popup destroyed");
        }
    }

    private void OnDisable()
    {
        // Clean up when the object is disabled
        CancelInvoke(nameof(StartFade));
        StopAllCoroutines();
    }
}