using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIBuilder
{
    public static GameObject BuildSettingsPanel(List<ISetting> settings, Shape targetShape)
    {
        if (settings == null || settings.Count == 0) return null;

        GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);

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
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        foreach (ISetting setting in settings)
        {
            // 🔥 Pass targetShape to settings that require it!
            
            GameObject settingUI = setting.GetUI();
            if (settingUI != null)
            {
                settingUI.transform.SetParent(content.transform, false);
            }
        }

        return panel;
    }

}
