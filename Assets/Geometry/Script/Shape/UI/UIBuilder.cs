using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIBuilder
{
    public static GameObject BuildSettingsPanel(Shape targetShape)
    {
        if (targetShape == null) return null;
        if (targetShape.GetSettings() == null || targetShape.GetSettings().Count == 0) return null;

        GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0f);

        ScrollRect scrollRect = panel.AddComponent<ScrollRect>();
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.SetParent(panel.transform, false);
        scrollRect.content = contentRect;

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (ISetting setting in targetShape.GetSettings())
        {
            setting.Update();
            GameObject settingUI = setting.GetUI();
            if (settingUI != null)
            {  
                RectTransform settingRect = settingUI.GetComponent<RectTransform>();
                settingRect.anchorMin = new Vector2(0, 1);
                settingRect.anchorMax = new Vector2(1, 1);
                settingRect.pivot = new Vector2(0.5f, 1);
                settingUI.transform.SetParent(content.transform, false);
                settingRect.sizeDelta = new Vector2(0, settingRect.sizeDelta.y); // Auto-scale width
            }
        }

        return panel;
    }
}
