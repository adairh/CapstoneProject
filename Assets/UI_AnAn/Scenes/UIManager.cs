using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnAn
{
    public class UIManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement homeTab;
        private VisualElement filesTab;

        private Button btnHome;
        private Button btnFiles;
        private Button btnJoinRoom;
        private Button btnCreateRoom;
        private Button btnTips;
        private Button btnDrawing;

        void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument component not found!");
                return;
            }

            var root = uiDocument.rootVisualElement;

            homeTab = root.Q<VisualElement>("HomeTab");
            filesTab = root.Q<VisualElement>("FilesTab");

            btnHome = root.Q<Button>("btnHome");
            btnFiles = root.Q<Button>("btnFiles");

            if (homeTab == null || filesTab == null)
            {
                Debug.LogError("HomeTab or FilesTab not found!");
                return;
            }

            btnHome.clicked += () => SwitchTab(true);
            btnFiles.clicked += () => SwitchTab(false);

            SwitchTab(true);
        }

        private void SwitchTab(bool isHome)
        {
            homeTab.style.display = isHome ? DisplayStyle.Flex : DisplayStyle.None;
            filesTab.style.display = isHome ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

