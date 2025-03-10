using UnityEngine;
using System.Collections.Generic;

public class SpawnPanel
{
    private GameObject spawnedPanel;
    private RectTransform canvasRect;
    private Canvas canvas;

    public SpawnPanel()
    {
        canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("No Canvas found in the scene!");
        }
    }

    public void SpawnPanelAtTop(Shape shape)
    {
        if (canvasRect == null || UIManager.Instance == null) return;

        // Destroy existing panel before spawning a new one
        if (spawnedPanel != null)
        {
            Object.Destroy(spawnedPanel);
        }

        // Get settings for the shape
        List<ISetting> settings = shape.GetSettings();
        if (settings == null || settings.Count == 0) return;

        // Get panel prefab from UIManager
        if (!UIManager.Instance.UIPrefabs.TryGetValue("Panel", out GameObject panelPrefab) || panelPrefab == null)
        {
            Debug.LogError("Panel Prefab is missing in UIManager!");
            return;
        }

        // Instantiate UI panel at the top of the canvas
        spawnedPanel = Object.Instantiate(panelPrefab, canvas.transform);
        RectTransform panelRect = spawnedPanel.GetComponent<RectTransform>();

        panelRect.anchorMin = new Vector2(0.5f, 1f); // Top center
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -20); // Slightly below the top

        // Attach settings UI dynamically
        GameObject settingsPanel = UIBuilder.BuildSettingsPanel(shape);
        settingsPanel.transform.SetParent(spawnedPanel.transform, false);
        settingsPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        settingsPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        settingsPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        // Adjust panel size dynamically
        AdjustPanelSize(panelRect, settings.Count);

        // Add close-on-click-outside behavior
        spawnedPanel.AddComponent<PanelCloser>();
    }

    private void AdjustPanelSize(RectTransform panelRect, int settingCount)
    {
        var pixelRect = canvas.pixelRect;
        float panelWidth = pixelRect.width * 0.7f; // Increase to 75% of canvas width
        float panelHeight = pixelRect.height * 0.1f + (settingCount * 80f); // Increase base height and per-setting height

        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
    }

}
