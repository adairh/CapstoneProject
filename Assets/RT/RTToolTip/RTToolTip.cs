using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Attach to GUI object for tooltip.  Needs RTToolTipManger running somewhere as well

//By Seth A. Robinson, 2022

public class RTToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string _text = "Change this text to the tooltip you want!";
    private TextAlignment _alignment = TextAlignment.Center;

    private bool _bUsingCustomLocation;

    // Start is called before the first frame update
    private float _delayTimer;
    private Vector3 _vCustomLocation = new(0, 0, 0);

    private GameObject m_tipInstance;

    private void Start()
    {
    }


    // Update is called once per frame
    private void Update()
    {
        if (_delayTimer != 0 && _delayTimer < Time.time)
        {
            ShowTip(true);
            _delayTimer = 0;
        }
    }

    //stop showing the tip when this component is removed or destroyed
    private void OnDestroy()
    {
        ShowTip(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _delayTimer = Time.time + RTToolTipManager.Get().m_delayBeforeShowingSeconds;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ShowTip(false);
        _delayTimer = 0;
    }

    public void SetCustomLocationSetup(Vector3 vPos)
    {
        _vCustomLocation = vPos;
        _bUsingCustomLocation = true;
    }

    public void SetAlignment(TextAlignment alignment)
    {
        _alignment = alignment;
    }

    private void ShowTip(bool bNew)
    {
        if (bNew)
        {
            if (m_tipInstance == null)
            {
                var tipManager = RTToolTipManager.Get();
                //create it
                m_tipInstance = Instantiate(tipManager.m_toolTipPrefab, gameObject.transform);
                var bg = RTUtil.FindInChildrenIncludingInactive(m_tipInstance, "BG");
                var textObject = RTUtil.FindInChildrenIncludingInactive(m_tipInstance, "Text");
                var textComp = textObject.GetComponent<TMP_Text>();
                textComp.text = _text;
                var _tipPrefabScript = m_tipInstance.GetComponent<RTToolTipPrefabScript>();
                if (_tipPrefabScript == null)
                    Debug.LogError("Prefab doesn't have RTToolTipPrefabScript attached, WHY?!");

                if (_bUsingCustomLocation) m_tipInstance.transform.position = _vCustomLocation;

                _tipPrefabScript.SetAlignment(_alignment);

                //can't position the canvas itself, so we'll grab its child
                var rawImage = bg.GetComponent<RawImage>();

                var myCanvas = gameObject.GetComponentInParent<Canvas>();

                if (!myCanvas || myCanvas.renderMode == RenderMode.WorldSpace)
                {
                    var cam = RTUtil.FindObjectOrCreate("Camera").GetComponent<Camera>();
                    m_tipInstance.transform
                        .SetParent(null); //move to root, we can't be a screen canvas attached to a world canvas

                    //special handling, we need to convert the camera position to screenspace first
                    var screenPos = cam.WorldToScreenPoint(transform.position);

                    if (_bUsingCustomLocation) screenPos = cam.WorldToScreenPoint(_vCustomLocation);

                    m_tipInstance.transform.position = screenPos;
                }
            }
        }
        else
        {
            //stop showing it
            if (m_tipInstance)
            {
                Destroy(m_tipInstance);
                m_tipInstance = null;
            }
        }
    }
}