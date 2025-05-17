using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RTSimpleNavBar : MonoBehaviour
{
    public GameObject _navOptionPrefab;
    private Button _mainNavButton;

    private bool _menuIsExpanded;

    private readonly List<Option> options = new();

    // Start is called before the first frame update
    private void Awake()
    {
        _mainNavButton = GetComponent<Button>();
    }


    public void Reset()
    {
        foreach (var option in options) Destroy(option._Obj);
        options.Clear();
    }

    private void Start()
    {
        _mainNavButton.onClick.AddListener(OnClickedTopNavButton);
    }

    // Update is called once per frame
    private void Update()
    {
        //OPTIMIZE:  Probably would be smarter to hook into a mousedown callback when the menu is opened, would avoid
        //checking every frame. Not gonna worry about it for < 1000 objects though

        if (_menuIsExpanded)
            if (Input.GetMouseButtonDown(0))
                RTMessageManager.Get().Schedule(0.2f, CloseMenu);
    }

    public void ExpandMenu()
    {
        Debug.Log("Expanding menu...");
        var buttonCount = 0;
        var vertPaddingFromTheTop = 3f;

        var topButtonRect = _mainNavButton.GetComponent<RectTransform>();


        //Expand the menu by instantiating the options using the prefab
        foreach (var option in options)
        {
            option._Obj = Instantiate(_navOptionPrefab, transform);
            var buttonText = option._Obj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = option._name;
            var toolTip = option._Obj.GetComponentInChildren<RTToolTip>();
            toolTip._text = option._toolTip;
            // Resize the button to fit the text
            var padding = 10f;
            var preferredWidth = buttonText.preferredWidth + padding * 2;
            var preferredHeight = buttonText.preferredHeight;
            var buttonRect = option._Obj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(preferredWidth, preferredHeight);

            // Left-justify the button text
            buttonRect.pivot = new Vector2(0, 0);

            option._Obj.GetComponent<Button>().onClick.AddListener(option.OnClick);
            option._Obj.GetComponent<Button>().onClick.AddListener(CloseMenu);


            //Make the object we created a child of us, so the proper scale etc is applied
            option._Obj.transform.SetParent(transform);

            option._Obj.transform.localPosition = Vector3.zero;
            option._Obj.transform.localRotation = Quaternion.identity;

            buttonCount++;

            // Move the button down by its height times the button count
            var buttonHeight = buttonRect.rect.height;
            //buttonRect.sizeDelta.x / 2f
            buttonRect.anchoredPosition = new Vector2(-(topButtonRect.rect.width / 2),
                -buttonHeight / 2f - (buttonCount * buttonHeight + vertPaddingFromTheTop));

            //setup the Tooltip to show to a custom location, otherwise it's kind of annoying how it will appear over
            //other options, visually distracting

            var tipScript = option._Obj.GetComponent<RTToolTip>();
            if (tipScript != null)
            {
                Vector2 vOffset = new Vector3(topButtonRect.rect.xMin, topButtonRect.rect.yMax);
                Vector3 vOffsetWorld;
                var cam = RTUtil.FindObjectOrCreate("Camera").GetComponent<Camera>();

                vOffsetWorld = topButtonRect.transform.TransformPoint(new Vector3(vOffset.x, vOffset.y, 0));
                tipScript.SetCustomLocationSetup(vOffsetWorld);
                tipScript.SetAlignment(TextAlignment.Left);
            }
            else
            {
                Debug.LogWarning("This should have an RTTooltip attached!");
            }
        }

        _menuIsExpanded = true;
    }

    public void CloseMenu()
    {
        Debug.Log("Closing menu...");

        //Close the menu by destroying the options
        foreach (var option in options)
        {
            Destroy(option._Obj);
            option._Obj = null;
        }

        _menuIsExpanded = false;
    }

    public void OnClickedTopNavButton()
    {
        if (_menuIsExpanded)
            CloseMenu();
        else
            ExpandMenu();
    }

    public void AddOption(string name, Action onClickCallback, string toolTip = "")
    {
        var option = ScriptableObject.CreateInstance<Option>();
        option._name = name;
        option._toolTip = toolTip;
        option._onClickCallback = onClickCallback;
        options.Add(option);
    }


    public class Option : ScriptableObject
    {
        public string _name;
        public string _toolTip;
        public GameObject _Obj;
        public Action _onClickCallback;

        public void OnClick()
        {
            _onClickCallback();
        }
    }
}