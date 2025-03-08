using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIBuilder
{
    public static GameObject BuildSettingsPanel(List<ISetting> settings, Transform parent = null)
    {
        if (settings == null || settings.Count == 0) return null;

        // Create a panel to contain settings UI
        GameObject panel = new GameObject("SettingsPanel");
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        Image panelImage = panel.AddComponent<Image>(); // Background
        panelImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent background

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 10;

        // Add Scroll View for better usability
        ScrollRect scrollRect = panel.AddComponent<ScrollRect>();
        GameObject content = new GameObject("Content");
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.SetParent(panel.transform, false);
        layout.transform.SetParent(content.transform, false);
        scrollRect.content = contentRect;

        if (parent != null) panel.transform.SetParent(parent, false);

        foreach (ISetting setting in settings)
        {
            GameObject settingUI = setting.GetUI();
            if (settingUI != null)
            {
                settingUI.transform.SetParent(content.transform, false);
            }
        }

        return panel;
    }
}