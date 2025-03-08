using System.Collections.Generic;
using UnityEngine;

public class ShapeClickHandler : MonoBehaviour
{
    private Shape shape; // Reference to the shape
    private GameObject spawnedPanel;
    private RectTransform canvasRect;
    private Canvas canvas;

    public void SetShape(Shape shape)
    {
        this.shape = shape;
        while (this.shape.Parent != null)
        {
            this.shape = this.shape.Parent;
        }
    }

    private void Start()
    {
        // Get Canvas from the scene
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

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            SpawnAtMousePosition();
        }
    }

    private void SpawnAtMousePosition()
    {
        if (canvasRect == null) return;

        // Get the clicked shape
        Shape clickedShape = ShapeStorage.GetShapeByID(shape.GO.name);
        if (clickedShape == null) return;

        
        
        // Get panelPrefab from UIManager
        Dictionary<string, GameObject> uis = UIManager.Instance.UIPrefabs;
        
        GameObject panelPrefab = uis["Panel"];

        
        if (panelPrefab == null)
        {
            Debug.LogError("Panel Prefab is missing in UIManager!");
            return;
        }

        if (spawnedPanel != null) Destroy(spawnedPanel);

        // Generate UI settings panel from shape settings
        List<ISetting> settings = clickedShape.GetSettings();
        GameObject settingsPanel = UIBuilder.BuildSettingsPanel(settings);

        // Instantiate UI panel
        spawnedPanel = Instantiate(panelPrefab, canvas.transform);
        RectTransform panelRect = spawnedPanel.GetComponent<RectTransform>();

        // Set panel position to mouse position
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out anchoredPos);
        panelRect.anchoredPosition = anchoredPos;

        // Attach settings UI to the spawned panel
        // UIManager.Instance.ShowSettingsPanel(settingsPanel);
        // Debug.LogWarning(clickedShape);

    }
}
