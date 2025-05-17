using TMPro;
using UnityEngine;

public class RTToolTipPrefabScript : MonoBehaviour
{
    public GameObject _backGround;
    public TMP_Text _textObj;

    public CanvasGroup _canvasGroup;

    // Start is called before the first frame update
    private TextAlignment _alignment = TextAlignment.Center;
    private bool _bDidFirstUpdate;
    private bool _bNeedsUpdate = true;
    private Vector3 _originalPos;

    private void Start()
    {
        _canvasGroup.alpha = 0; //avoid a flicker while we change its position
        _originalPos = _backGround.transform.position;
    }


    // Update is called once per frame
    private void Update()
    {
        if (!_bDidFirstUpdate)
        {
            _bDidFirstUpdate = true;
            return;
        }

        if (_bNeedsUpdate)
        {
            Reposition();
            _bNeedsUpdate = false;
        }
    }

    private void OnEnable()
    {
        //Debug.Log("PrintOnEnable: script was enabled");
    }

    private void OnDisable()
    {
        //Debug.Log("PrintOnDisable: script was disabled");
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Debug.Log("Invisible");
    }

    public void SetAlignment(TextAlignment alignment)
    {
        _alignment = alignment;
    }

    private void Reposition()
    {
        _textObj.enabled = true;

        // _textObj.alignment = TMPro.TextAlignmentOptions.Left;
        var vPos = _originalPos;
        var rt = _backGround.GetComponent<RectTransform>();

        if (_alignment == TextAlignment.Left)
        {
            rt.pivot = new Vector2(0, 0);
            rt.ForceUpdateRectTransforms();
        }
        else
        {
            rt.ForceUpdateRectTransforms();

            //move up, and then we'll tweak that if it looks like it's off the screen
            vPos.y += 48;
        }


        var offscreenX = vPos.x + rt.offsetMin.x;
        var offscreenY = vPos.y - rt.offsetMin.y;

        if (offscreenX < 0)
            //move it to the right
            vPos.x += -offscreenX;

        if (vPos.x + rt.offsetMax.x > Screen.width)
            //move to the left a bit
            vPos.x += Screen.width - (vPos.x + rt.offsetMax.x);

        if (offscreenY > Screen.height)
            //move it below us, there is no room above
            vPos.y -= 48 + 24 + rt.offsetMax.y * 2;

        _backGround.transform.position = vPos;
        _canvasGroup.alpha = 1;
    }
}