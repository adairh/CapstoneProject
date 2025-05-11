using System.Collections;
using UnityEngine;

namespace Geometry.Script.UISCript
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Fade : MonoBehaviour
    {
        [Tooltip("Time in seconds to wait before starting fade out")]
        public float delay = 0.5f;
        [Tooltip("Duration of the fade out effect in seconds")]
        public float fadeDuration = 1f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            // Get or add a CanvasGroup for fading
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            // Begin the fade-out sequence
            StartCoroutine(FadeOutCoroutine());
        }

        private IEnumerator FadeOutCoroutine()
        {
            // Wait for the specified delay after app start
            yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                // Lerp alpha from 1 to 0
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            // Ensure fully invisible at the end
            canvasGroup.alpha = 0f;
            // Optionally disable the GameObject after fade out
            // gameObject.SetActive(false);
        }
    }
}