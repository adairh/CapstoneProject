using TMPro;
using UnityEngine;

public class RTQuickMessagePrefabScript : MonoBehaviour
{
    public GameObject _backGround;
    public TMP_Text _textObj;
    public CanvasGroup _canvasGroup;
    private bool _bDidFirstUpdate;
    private bool _bNeedsUpdate = true;

    private Vector3 _originalPos;

    // Start is called before the first frame update
    private void Start()
    {
        _canvasGroup.alpha = 0; //avoid a flicker while we change its position
        _originalPos = _backGround.transform.position;

        // RTMessageManager.Get().Schedule(1, this.Die);
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

    public void SetKillTime(float timeInSecondsBeforeKillingIt)
    {
        RTMessageManager.Get().Schedule(timeInSecondsBeforeKillingIt, Die);
    }


    private void Die()
    {
        Destroy(gameObject);
    }

    private void Reposition()
    {
        _textObj.enabled = true;

        var vPos = _originalPos;

        var rt = _backGround.GetComponent<RectTransform>();
        rt.ForceUpdateRectTransforms();
        //move up
        //vPos.y += 48;

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
            vPos.y -= 24 + rt.offsetMax.y * 2;

        _backGround.transform.position = vPos;
        _canvasGroup.alpha = 1;
    }
}