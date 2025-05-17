using TMPro;
using UnityEngine;

public class SetBuildDate : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        var tm = GetComponent<TextMeshProUGUI>();
        tm.text = "Compiled " + RTBuildInfo.Timestamp;
    }
}