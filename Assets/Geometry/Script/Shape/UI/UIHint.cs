using System.Collections;
using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class UIHint : MonoBehaviour
    {
        public static UIHint Instance;
        public TextMeshProUGUI hintText;
        
        private Coroutine tempHintCoroutine;
        
        private void Awake()
        {
            Instance = this;
            if (hintText != null)
                hintText.gameObject.SetActive(false); // Just hide text, keep object enabled
        }


        public static void Show(string message)
        {
            if (Instance != null && Instance.hintText != null)
            {
                Instance.hintText.text = message;
                Instance.hintText.gameObject.SetActive(true);
            }
        }

        public static void Hide()
        {
            if (Instance != null && Instance.hintText != null) Instance.hintText.gameObject.SetActive(false);
        }
        
        public static void ShowTemp(string msg, float seconds)
        {
            if (Instance == null) return;
            if (Instance.tempHintCoroutine != null)
                Instance.StopCoroutine(Instance.tempHintCoroutine);
            Instance.tempHintCoroutine = Instance.StartCoroutine(Instance.TempHint(msg, seconds));
        }

        private IEnumerator TempHint(string msg, float seconds)
        {
            hintText.text = msg;
            hintText.enabled = true;
            yield return new WaitForSeconds(seconds);
            hintText.enabled = false;
            tempHintCoroutine = null;
        }
    }
}