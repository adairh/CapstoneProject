using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpawnPanel : MonoBehaviour
{
    public GameObject panelPrefab; // Assign your panel prefab in the inspector
    private GameObject spawnedPanel;
    private RectTransform canvasRect;
    private Canvas canvas;

    void Start()
    {
        // Find the Canvas in the scene
        canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("No Canvas found in the scene!");
        }
    }

    void Update()
    {
        // Right-click to spawn panel
        if (Input.GetMouseButtonDown(1)) 
        {
            SpawnAtMousePosition();
        }

        // Left-click outside the panel to destroy it
        if (spawnedPanel != null && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI(spawnedPanel))
            {
                Destroy(spawnedPanel);
            }
        }
    }

    void SpawnAtMousePosition()
    {
        if (panelPrefab == null || canvasRect == null) return;

        // Destroy existing panel if present
        if (spawnedPanel != null) Destroy(spawnedPanel);

        // Instantiate the panel inside the Canvas
        spawnedPanel = Instantiate(panelPrefab, canvas.transform);
        RectTransform panelRect = spawnedPanel.GetComponent<RectTransform>();

        // Convert mouse position to UI position
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out anchoredPos);

        // Set position in UI space
        panelRect.anchoredPosition = anchoredPos;
    }

    bool IsPointerOverUI(GameObject uiElement)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == uiElement || result.gameObject.transform.IsChildOf(uiElement.transform))
            {
                return true;
            }
        }
        return false;
    }
}
