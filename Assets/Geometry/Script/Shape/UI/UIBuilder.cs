using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Manipulator
{
    public class UIBuilder
    {
        public static GameObject BuildSettingsPanel(Shape targetShape)
        {
            if (targetShape == null || targetShape.GetSettings() == null || targetShape.GetSettings().Count == 0)
                return null;

            // Create the main panel (scroll view container)
            GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panel.AddComponent<CanvasRenderer>();
            panel.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            panelRect.sizeDelta = new Vector2(300, 400); // Fixed size
            panelRect.anchorMin = new Vector2(0.5f, 0.5f); // Center
            panelRect.anchorMax = new Vector2(0.5f, 0.5f); // Center
            panelRect.pivot = new Vector2(0.5f, 0.5f); // Center
            //panelRect.anchoredPosition = Vector2.zero; // Center position

            // ScrollRect
            ScrollRect scrollRect = panel.AddComponent<ScrollRect>();
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.elasticity = 0.1f;

            // Create the viewport
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.SetParent(panel.transform, false);
            viewportRect.anchorMin = new Vector2(0, 0);
            viewportRect.anchorMax = new Vector2(1, 1);
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.1f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = viewportRect;

            // Create content object
            GameObject content = new GameObject("Content", typeof(RectTransform));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.SetParent(viewport.transform, false);
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            scrollRect.content = contentRect;
            scrollRect.verticalNormalizedPosition = 1f;

            // Add layout components
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 5f;
            layout.padding = new RectOffset(5, 5, 60, 5);
            layout.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add settings UI elements
            foreach (ISetting setting in targetShape.GetSettings())
            {
                setting.Update();
                GameObject settingUI = setting.GetUI();
                if (settingUI != null)
                {
                    settingUI.transform.SetParent(content.transform, false);

                    LayoutElement le = settingUI.GetComponent<LayoutElement>();
                    if (le == null)
                    {
                        le = settingUI.AddComponent<LayoutElement>();
                        le.preferredHeight = 30f;
                        le.preferredWidth = 280f;
                    }
                }
            }

            return panel;
        }
    }
}