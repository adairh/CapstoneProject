/// <summary>
/// By Khoa
/// This script is used to show a notification when a player joins.
/// <!---->

using System.Collections;
using TMPro;
using UnityEngine;

namespace Khoa
{
    public class NotificationPopup : MonoBehaviour
    {
        public TextMeshProUGUI clientNameText;
        private CanvasGroup canvasGroup;
        private readonly float displayTime = 3f;
        private readonly float fadeTime = 1f;

        private void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
        }

        public void SetMessage(string message)
        {
            if (clientNameText != null)
                clientNameText.text = message;
            //Debug.Log($"Notification set with message: {message}");
            else
                Debug.LogError("ClientNameText is not assigned in NotificationPopup!");
            Invoke(nameof(StartFade), displayTime);
        }

        private void StartFade()
        {
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            var elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}