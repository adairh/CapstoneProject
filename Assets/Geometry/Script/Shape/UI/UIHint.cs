using System.Collections;
using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class UIHint : MonoBehaviour
    {
        public static UIHint Instance;
        public TextMeshProUGUI hintText;

        [Header("VFX Settings")]
        public float fadeTime = 0.25f;
        public float scaleUp = 1.1f;
        public float scaleTime = 0.15f;

        private Coroutine tempHintCoroutine;

        private void Awake()
        {
            Instance = this;
            if (hintText != null)
                hintText.gameObject.SetActive(false);
        }

        public static void Show(string message)
        {
            if (Instance == null || Instance.hintText == null) return;
            Instance.hintText.text = message;
            Instance.hintText.gameObject.SetActive(true);
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.FadeInVFX());
        }

        public static void Hide()
        {
            if (Instance == null || Instance.hintText == null) return;
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.FadeOutVFX());
        }

        public static void ShowTemp(string msg, float seconds)
        {
            if (Instance == null) return;
            if (Instance.tempHintCoroutine != null)
                Instance.StopCoroutine(Instance.tempHintCoroutine);
            Instance.tempHintCoroutine = Instance.StartCoroutine(Instance.TempHintVFX(msg, seconds));
        }

        // Fade and scale in
        private IEnumerator FadeInVFX()
        {
            Color c = hintText.color;
            c.a = 0;
            hintText.color = c;
            hintText.transform.localScale = Vector3.one * scaleUp;

            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0, 1, t / fadeTime);
                hintText.color = c;
                hintText.transform.localScale = Vector3.Lerp(Vector3.one * scaleUp, Vector3.one, t / scaleTime);
                yield return null;
            }
            c.a = 1;
            hintText.color = c;
            hintText.transform.localScale = Vector3.one;
        }

        // Fade and scale out
        private IEnumerator FadeOutVFX()
        {
            Color c = hintText.color;
            c.a = 1;
            hintText.color = c;
            hintText.transform.localScale = Vector3.one;

            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1, 0, t / fadeTime);
                hintText.color = c;
                hintText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * scaleUp, t / scaleTime);
                yield return null;
            }
            c.a = 0;
            hintText.color = c;
            hintText.gameObject.SetActive(false);
        }

        // Show pop + fade for temp hint
        private IEnumerator TempHintVFX(string msg, float seconds)
        {
            hintText.text = msg;
            hintText.gameObject.SetActive(true);

            // Pop in
            Color c = hintText.color; c.a = 1; hintText.color = c;
            hintText.transform.localScale = Vector3.one * scaleUp;
            float t = 0;
            while (t < scaleTime)
            {
                t += Time.deltaTime;
                hintText.transform.localScale = Vector3.Lerp(Vector3.one * scaleUp, Vector3.one, t / scaleTime);
                yield return null;
            }
            hintText.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(seconds);

            // Fade out
            t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1, 0, t / fadeTime);
                hintText.color = c;
                hintText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * scaleUp, t / scaleTime);
                yield return null;
            }
            c.a = 0;
            hintText.color = c;
            hintText.gameObject.SetActive(false);
            tempHintCoroutine = null;
        }
    }
}
