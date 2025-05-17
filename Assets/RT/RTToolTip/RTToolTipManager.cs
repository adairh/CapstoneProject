using UnityEngine;

//Just need one of these added to an object somewhere, doesn't matter where. Oh, and it needs to have the prefab set to
//the RTNotepadPrefab file.
//Then you just add RTToolTip.cs to any GUI object that needs a tooltip.

//By Seth A. Robinson, 2022

public class RTToolTipManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static RTToolTipManager m_this;

    public GameObject m_toolTipPrefab;
    public float m_delayBeforeShowingSeconds = 0.5f;

    private void Awake()
    {
        m_this = this;
    }

    public static RTToolTipManager Get()
    {
        return m_this;
    }
}