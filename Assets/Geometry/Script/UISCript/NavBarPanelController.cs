using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NavBarPanelController : MonoBehaviour
{
    [System.Serializable]
    public class NavItem
    {
        public Button button;       // Assign in Inspector
        public GameObject panel;    // Assign in Inspector (panel root, same as button’s parent in your prefab)
        public Image buttonImage;   // Assign in Inspector (button background image)
    }

    public List<NavItem> navItems = new List<NavItem>();

    public bool onlyOnePanelActive = true;
    public Color activeColor = new Color32(0xFF, 0xED, 0x00, 0xFF);    // #FFED00
    public Color inactiveColor = new Color32(0x08, 0x21, 0x1E, 0xFF);  // #08211E

    private void Awake()
    {
        foreach (var item in navItems)
        {
            var localItem = item; // Local copy for closure safety
            item.button.onClick.AddListener(() => OnNavButtonClicked(localItem));
        }
    }

    private void Start()
    {
        // Show first, hide rest if onlyOnePanelActive
        if (onlyOnePanelActive && navItems.Count > 0)
        {
            for (int i = 0; i < navItems.Count; i++)
                navItems[i].panel.SetActive(i == 0);
        }
        UpdateButtonColors();
    }

    void OnNavButtonClicked(NavItem clicked)
    {
        if (onlyOnePanelActive)
        {
            foreach (var item in navItems)
                item.panel.SetActive(item == clicked);
        }
        else
        {
            clicked.panel.SetActive(!clicked.panel.activeSelf);
        }
        UpdateButtonColors();
    }

    void UpdateButtonColors()
    {
        foreach (var item in navItems)
        {
            bool isActive = item.panel.activeSelf;
            if (item.buttonImage != null)
                item.buttonImage.color = isActive ? activeColor : inactiveColor;
        }
    }
}