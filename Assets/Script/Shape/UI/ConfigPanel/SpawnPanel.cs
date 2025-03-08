using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SpawnPanel : MonoBehaviour
{
    public GameObject panelPrefab;
    private GameObject spawnedPanel;
    private RectTransform canvasRect;
    private Canvas canvas;

    void Start()
    {
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
        if (Input.GetMouseButtonDown(1)) 
        {
            SpawnAtMousePosition();
        }

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

        if (spawnedPanel != null) Destroy(spawnedPanel);

        // Example settings list (this should be obtained dynamically)
        List<ISetting> settings = GetSettingsForShape();

        // Build UI panel from settings
        GameObject settingsPanel = UIBuilder.BuildSettingsPanel(settings);

        spawnedPanel = Instantiate(panelPrefab, canvas.transform);
        RectTransform panelRect = spawnedPanel.GetComponent<RectTransform>();

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out anchoredPos);

        panelRect.anchoredPosition = anchoredPos;

        // Attach UI to the settings panel
        // UIManager.Instance.ShowSettingsPanel(settingsPanel);

        //Shape clickedShape = ShapeStorage.GetShapeByID(gameObject.name);
        //Debug.Log("Clicked " + clickedShape);
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

    private List<ISetting> GetSettingsForShape()
    {
        // Example: Replace with actual logic to fetch settings
        return new List<ISetting>
        {
            new RadiusSetting(5f),
            // Add more settings dynamically
        };
    }
}
