using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Manipulator
{
    public class UIBuilder
    {
        public static GameObject BuildSettingsPanel(Shape targetShape)
        {
            if (targetShape == null) return null;
            if (targetShape.GetSettings() == null || targetShape.GetSettings().Count == 0) return null;

            // Create main panel
            GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panel.AddComponent<CanvasRenderer>();
            
            // Add background to panel
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            // Create scroll view
            ScrollRect scrollRect = panel.AddComponent<ScrollRect>();
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20f;
            scrollRect.elasticity = 0.1f;
            scrollRect.decelerationRate = 0.135f;

            // Create content area
            GameObject content = new GameObject("Content", typeof(RectTransform));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.SetParent(panel.transform, false);
            
            // Set anchors for content to stretch horizontally, top-aligned
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);
            scrollRect.content = contentRect;

            // Add layout group and size fitter
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 5f;
            layout.padding = new RectOffset(5, 5, 5, 5);

            var sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Add settings
            foreach (ISetting setting in targetShape.GetSettings())
            {
                setting.Update();
                GameObject settingUI = setting.GetUI();
                if (settingUI != null)
                {
                    settingUI.transform.SetParent(content.transform, false);

                    // Ensure each setting UI has a LayoutElement with a preferred height
                    var layoutElem = settingUI.GetComponent<LayoutElement>();
                    if (layoutElem == null)
                        layoutElem = settingUI.AddComponent<LayoutElement>();
                    layoutElem.preferredHeight = 40; // or whatever fits your design
                }
            }

            return panel;
        }
    }
}