using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Manipulator
{

    public class MeasureInfoBar : MonoBehaviour
    {
        public static MeasureInfoBar Instance;

        public TextMeshProUGUI label;
        public Button editButton;
        public GameObject barRoot;

        private System.Action onEdit;

        void Awake() => Instance = this;

        public void Show(string message, System.Action onEditClicked)
        {
            label.text = message;
            onEdit = onEditClicked;
            barRoot.SetActive(true);
            editButton.onClick.RemoveAllListeners();
            if (onEdit != null) editButton.onClick.AddListener(() => onEdit());
        }

        public void Hide()
        {
            barRoot.SetActive(false);
            onEdit = null;
        }
    }
}