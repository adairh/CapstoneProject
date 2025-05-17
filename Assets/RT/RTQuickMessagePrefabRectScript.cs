using TMPro;
using UnityEngine;

public class RTQuickMessagePrefabRectScript : MonoBehaviour
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

        _backGround.transform.position = vPos;
        _canvasGroup.alpha = 1;
    }
}