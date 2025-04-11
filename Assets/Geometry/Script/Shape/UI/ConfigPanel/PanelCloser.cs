using UnityEngine;
using UnityEngine.EventSystems;

public class PanelCloser : MonoBehaviour
{
    private bool isInitialized = false;

    private void Start()
    {
        // Small delay before activation to avoid detecting the click that spawned the panel
        Invoke(nameof(EnableCloseDetection), 0.1f);
    }

    private void EnableCloseDetection()
    {
        isInitialized = true;
    }

    private void Update()
    {
        // Wait until initialization to prevent immediate destruction on spawn
        if (!isInitialized) return;

        // Check if user clicked anywhere outside the UI
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            if (!IsPointerOverUIElement())
            {
                Destroy(gameObject); // Destroy the panel if clicked outside
            }
        }
    }

    private bool IsPointerOverUIElement()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}