
using UnityEngine;
using UnityEngine.UI;

namespace Manipulator
{
    public class PrebuiltSystemSetup : MonoBehaviour
    {
        [ContextMenu("Setup Prebuilt System")]
        public void Setup()
        {
            if (FindObjectOfType<PrebuiltDrawingHandler>() == null)
            {
                var handlerGO = new GameObject("PrebuiltDrawingHandler");
                handlerGO.AddComponent<PrebuiltDrawingHandler>();
                Debug.Log("✅ Added PrebuiltDrawingHandler");
            }

            if (FindObjectOfType<Canvas>() == null)
            {
                var canvasGO = new GameObject("Canvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ Created Canvas");
            }

            if (FindObjectOfType<PrebuiltSpawnPanel>() == null)
            {
                var panelGO = new GameObject("PrebuiltSpawnPanel");
                panelGO.transform.SetParent(FindObjectOfType<Canvas>().transform);

                var rect = panelGO.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(400, 300);
                panelGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

                var panel = panelGO.AddComponent<PrebuiltSpawnPanel>();
                panelGO.SetActive(false);

                Debug.Log("✅ Created PrebuiltSpawnPanel placeholder (setup your inputs manually)");
            }
        }
    }
}
