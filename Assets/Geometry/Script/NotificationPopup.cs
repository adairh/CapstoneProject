
/// <summary>
/// By Khoa
/// This script is used to show a notification when a player joins.
/// <!---->

using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
namespace Khoa
{
    public class NotificationPopup : MonoBehaviour
    {
        public TextMeshProUGUI clientNameText;
        private float displayTime = 3f;
        private float fadeTime = 1f;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
        }

        public void SetPlayerId(string playerId)
        {
            if (clientNameText != null)
            {
                clientNameText.text = $"Client Joined: {playerId}";
                Debug.Log($"Notification set with PlayerId: {playerId}");
            }
            else
            {
                Debug.LogError("ClientNameText is not assigned in NotificationPopup!");
            }
            Invoke(nameof(StartFade), displayTime);
        }

        private void StartFade()
        {
            StartCoroutine(FadeOut());
        }

        private System.Collections.IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
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

