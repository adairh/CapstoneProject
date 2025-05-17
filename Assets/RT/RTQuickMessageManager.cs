using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Just need one of these added to an object somewhere, doesn't matter where. Oh, and it needs to have the prefab set to
//the RTQuickMessagePrefab file.
//Then you just add RMessageManager.cs to any GUI object that needs a tooltip.

public class RTQuickMessageManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static RTQuickMessageManager m_this;
    public GameObject m_quickMessagePrefab;
    private readonly bool m_bOnlyAllowOne = true;
    private GameObject m_lastObjCreated;

    private void Awake()
    {
        m_this = this;
    }

    public static RTQuickMessageManager Get()
    {
        return m_this;
    }

    public void ShowMessage(string msg, float forceDisplayTime = 0)
    {
        var vPos = new Vector2(Screen.width / 2, Screen.height - Screen.height * 0.9f);
        ShowMessage(msg, vPos, forceDisplayTime);
    }

    public float GetDelayByMessageLength(string msg, float timeMod = 1.0f)
    {
        var delay = msg.Length * 0.16f;
        delay *= timeMod;
        delay = Mathf.Clamp(delay, 1, float.MaxValue);
        return delay;
    }

    public void ShowMessage(string msg, Vector2 vPos, float forceDisplayTime = 0)
    {
        var vFinalPos = new Vector3(vPos.x, vPos.y, 1);

        //create it
        var go = Instantiate(Get().m_quickMessagePrefab, null);

        if (m_bOnlyAllowOne && m_lastObjCreated != null)
        {
            Destroy(m_lastObjCreated);
            m_lastObjCreated = null;
        }

        m_lastObjCreated = go;
        var bg = RTUtil.FindInChildrenIncludingInactive(go, "BG");
        var textObject = RTUtil.FindInChildrenIncludingInactive(go, "Text");
        var textComp = textObject.GetComponent<TMP_Text>();
        textComp.text = msg;
        //can't position the canvas itself, so we'll grab its child
        var rawImage = bg.GetComponent<RawImage>();
        bg.transform.position = vFinalPos;
        var myCanvas = gameObject.GetComponentInParent<Canvas>();

        if (forceDisplayTime == 0) forceDisplayTime = GetDelayByMessageLength(msg);

        var script = go.GetComponent<RTQuickMessagePrefabScript>();

        if (script == null)
        {
            var scriptRect = go.GetComponent<RTQuickMessagePrefabRectScript>();
            scriptRect.SetKillTime(forceDisplayTime);
        }
        else
        {
            script.SetKillTime(forceDisplayTime);
        }
        /*
        if (!myCanvas || myCanvas.renderMode == RenderMode.WorldSpace)
        {
            var cam = RTUtil.FindObjectOrCreate("Camera").GetComponent<Camera>();
            go.transform.SetParent(null); //move to root, we can't be a screen canvas attached to a world canvas

            //special handling, we need to convert the camera position to screenspace first
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
            go.transform.position = screenPos;
        }
        */
    }
}