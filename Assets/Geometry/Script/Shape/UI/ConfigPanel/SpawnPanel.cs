using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SpawnPanel
    {
        private readonly Canvas canvas;
        private readonly RectTransform canvasRect;

        public SpawnPanel()
        {
            Debug.Log("[SpawnPanel] Constructor");
            //canvas = Object.FindObjectOfType<Canvas>();
            canvas = UIManager.Instance.CanvasSetting;
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
                Debug.Log($"[SpawnPanel] Found Canvas: {canvas.name}, canvasRect={canvasRect}");
            }
            else
            {
                Debug.LogError("[SpawnPanel] No Canvas found in the scene!");
            }
        }

        // Make currentPanel public static but readonly from outside
        public static GameObject CurrentPanel { get; private set; }

        // Add a public method to clear the panel
        public static void ClearCurrentPanel()
        {
            CurrentPanel = null;
        }

        public void SpawnPanelAtTop(Shape shape)
        {
            Debug.Log($"[SpawnPanel] SpawnPanelAtTop called for shape: {shape.name}");
            if (canvasRect == null || UIManager.Instance == null)
            {
                Debug.LogError("[SpawnPanel] Required components are null");
                return;
            }

            // Destroy any existing panel first
            if (CurrentPanel != null)
            {
                Debug.Log("[SpawnPanel] Destroying previous panel");
                Object.Destroy(CurrentPanel);
                CurrentPanel = null;
            }

            // Get settings for the shape
            var settings = shape.GetSettings();
            Debug.Log($"[SpawnPanel] shape.GetSettings() returned {settings?.Count ?? 0} entries");
            if (settings == null || settings.Count == 0)
            {
                Debug.LogWarning("[SpawnPanel] No settings to show, aborting");
                return;
            }

            // Get panel prefab from UIManager
            if (!UIManager.Instance.UIPrefabs.TryGetValue("Panel", out var panelPrefab) || panelPrefab == null)
            {
                Debug.LogError("[SpawnPanel] Panel Prefab named \"Panel\" is missing in UIManager!");
                return;
            }

            Debug.Log($"[SpawnPanel] Got panel prefab: {panelPrefab.name}");

            // Instantiate new panel
            CurrentPanel = Object.Instantiate(panelPrefab, canvas.transform);
            Debug.Log($"[SpawnPanel] Instantiated panel: {CurrentPanel.name}");
            var panelRect = CurrentPanel.GetComponent<RectTransform>();
            Debug.Log($"[SpawnPanel] panelRect after GetComponent: {panelRect}");

            // Set panel position and anchors
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0, -20);

            // Build and attach settings UI
            var settingsPanel = UIBuilder.BuildSettingsPanel(shape);
            if (settingsPanel == null)
            {
                Debug.LogError("[SpawnPanel] UIBuilder.BuildSettingsPanel returned null!");
            }
            else
            {
                Debug.Log($"[SpawnPanel] Built settingsPanel: {settingsPanel.name}");
                settingsPanel.transform.SetParent(CurrentPanel.transform, false);

                var rt = settingsPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(0.5f, 1);
                    Debug.Log("[SpawnPanel] settingsPanel RectTransform setup");
                }
                else
                {
                    Debug.LogWarning("[SpawnPanel] settingsPanel has no RectTransform!");
                }
            }

            // Adjust panel size dynamically
            AdjustPanelSize(panelRect, settings);
            Debug.Log($"[SpawnPanel] Panel size after Adjust: {panelRect.sizeDelta}");

            // Add close-on-click-outside behavior
            CurrentPanel.AddComponent<PanelCloser>();
            Debug.Log("[SpawnPanel] Added PanelCloser component");
        }

        private void AdjustPanelSize(RectTransform panelRect, List<ISetting> settings)
        {
            var pixelRect = canvas.pixelRect;
            var panelWidth = Mathf.Min(pixelRect.width * 0.7f, 400f); // Max width of 400 pixels
            var panelHeight = Mathf.Min(pixelRect.height * 0.8f, 600f); // Max height of 600 pixels

            // Set the panel size
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

            // Ensure the panel is properly anchored at the top
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0, -20);
        }
    }
}