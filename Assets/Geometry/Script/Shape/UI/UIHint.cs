using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class UIHint : MonoBehaviour
    {
        public static UIHint Instance;
        public TextMeshProUGUI hintText;

        private void Awake()
        {
            Instance = this;
            Hide();
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
    }
}