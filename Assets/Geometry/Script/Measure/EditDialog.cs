using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Manipulator
{

    public class EditDialog : MonoBehaviour
    {
        public static EditDialog Instance;

        public TMP_InputField inputField;
        public Button okButton, cancelButton;
        public GameObject dialogRoot;
        private System.Action<float> onConfirm;

        void Awake() => Instance = this;

        public void Show(float currentValue, System.Action<float> onConfirmEdit)
        {
            dialogRoot.SetActive(true);
            inputField.text = currentValue.ToString("F2");
            onConfirm = onConfirmEdit;

            okButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Hide);
        }

        void Confirm()
        {
            if (float.TryParse(inputField.text, out float val))
            {
                dialogRoot.SetActive(false);
                onConfirm?.Invoke(val);
            }
        }

        public void Hide()
        {
            dialogRoot.SetActive(false);
        }
    }
}