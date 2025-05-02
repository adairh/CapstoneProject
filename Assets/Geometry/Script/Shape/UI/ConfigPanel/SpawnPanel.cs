using UnityEngine;
using System.Collections.Generic;

namespace Manipulator
{
    public class SpawnPanel
    {
        private GameObject spawnedPanel;
        private RectTransform canvasRect;
        private Canvas canvas;

        public SpawnPanel()
        {
            Debug.Log("[SpawnPanel] Constructor");
            canvas = Object.FindObjectOfType<Canvas>();
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

        public void SpawnPanelAtTop(Shape shape)
        {
            Debug.Log($"[SpawnPanel] SpawnPanelAtTop called for shape: {shape.Name}");
            if (canvasRect == null)
            {
                Debug.LogError("[SpawnPanel] canvasRect is null → cannot spawn panel");
                return;
            }
            if (UIManager.Instance == null)
            {
                Debug.LogError("[SpawnPanel] UIManager.Instance is null → cannot spawn panel");
                return;
            }

            // Destroy existing panel before spawning a new one
            if (spawnedPanel != null)
            {
                Debug.Log("[SpawnPanel] Destroying previous panel");
                Object.Destroy(spawnedPanel);
            }

            // Get settings for the shape
            List<ISetting> settings = shape.GetSettings();
            Debug.Log($"[SpawnPanel] shape.GetSettings() returned {settings?.Count ?? 0} entries");
            if (settings == null || settings.Count == 0)
            {
                Debug.LogWarning("[SpawnPanel] No settings to show, aborting");
                return;
            }

            // Get panel prefab from UIManager
            if (!UIManager.Instance.UIPrefabs.TryGetValue("Panel", out GameObject panelPrefab) || panelPrefab == null)
            {
                Debug.LogError("[SpawnPanel] Panel Prefab named \"Panel\" is missing in UIManager!");
                return;
            }
            Debug.Log($"[SpawnPanel] Got panel prefab: {panelPrefab.name}");

            // Instantiate UI panel at the top of the canvas
            spawnedPanel = Object.Instantiate(panelPrefab, canvas.transform);
            Debug.Log($"[SpawnPanel] Instantiated panel: {spawnedPanel.name}");
            RectTransform panelRect = spawnedPanel.GetComponent<RectTransform>();
            Debug.Log($"[SpawnPanel] panelRect after GetComponent: {panelRect}");

            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot     = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0, -20);

            // Attach settings UI dynamically
            GameObject settingsPanel = UIBuilder.BuildSettingsPanel(shape);
            if (settingsPanel == null)
            {
                Debug.LogError("[SpawnPanel] UIBuilder.BuildSettingsPanel returned null!");
            }
            else
            {
                Debug.Log($"[SpawnPanel] Built settingsPanel: {settingsPanel.name}");
                settingsPanel.transform.SetParent(spawnedPanel.transform, false);

                var rt = settingsPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot     = new Vector2(0.5f, 1);
                    Debug.Log($"[SpawnPanel] settingsPanel RectTransform setup");
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
            spawnedPanel.AddComponent<PanelCloser>();
            Debug.Log("[SpawnPanel] Added PanelCloser component");
        }

        private void AdjustPanelSize(RectTransform panelRect, List<ISetting> settings)
        {
            var pixelRect = canvas.pixelRect;
            float panelWidth  = pixelRect.width  * 0.7f;
            float panelHeight = pixelRect.height * 0.1f;

            Debug.Log($"[SpawnPanel] Canvas pixelRect={pixelRect}, initial size={panelWidth}x{panelHeight}");
            // nếu muốn cộng thêm từng setting:
            // foreach (ISetting i in settings)
            //     panelHeight += i.Height();

            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        }
    }
}
