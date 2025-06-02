using UnityEngine;
using UnityEngine.EventSystems;

namespace Manipulator
{
    public class PanelCloser : MonoBehaviour
    {
        private bool isInitialized;

        private void Start()
        {
            // Small delay before activation
            Invoke(nameof(EnableCloseDetection), 0.1f);
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                if (!IsPointerOverUIElement())
                {
                    // Use the public method to clear the panel reference
                    if (gameObject == SpawnPanel.CurrentPanel) SpawnPanel.ClearCurrentPanel();
                    Destroy(gameObject);
                    UIManager.Instance.InspectorRoot.SetActive(false);

                }
        }

        private void OnDestroy()
        {
            // Use the public method to clear the panel reference
            if (gameObject == SpawnPanel.CurrentPanel) SpawnPanel.ClearCurrentPanel();
        }

        private void EnableCloseDetection()
        {
            isInitialized = true;
        }

        private bool IsPointerOverUIElement()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}