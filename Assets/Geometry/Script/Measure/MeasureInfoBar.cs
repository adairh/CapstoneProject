using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Manipulator
{

    public class MeasureInfoBar : MonoBehaviour
    {
        public static MeasureInfoBar Instance;

        public TextMeshProUGUI label;
        public Button editButton;
        public GameObject barRoot;
        public GameObject panelInside;

        private System.Action onEdit;

        void Awake() => Instance = this;

        public void Show(string message, System.Action onEditClicked)
        {
            label.text = message;
            onEdit = onEditClicked;
            barRoot.SetActive(true);
            panelInside.SetActive(true);
            editButton.onClick.RemoveAllListeners();
            if (onEdit != null) editButton.onClick.AddListener(() => onEdit());
        }

        public void Hide()
        {
            panelInside.SetActive(false);
            barRoot.SetActive(false);
            //onEdit = null;
        }
        
        private void Update()
        {

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                if (!IsPointerOverUIElement())
                { 
                    if (barRoot.GetComponentInChildren<PanelCloser>() == null)
                        barRoot.SetActive(false);
                }
        }
        
        private bool IsPointerOverUIElement()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}