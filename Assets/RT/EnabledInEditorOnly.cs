using UnityEngine;

public class EnabledInEditorOnly : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
#if UNITY_EDITOR
        var e = 2;
        e = e + 1;
#else
        gameObject.SetActive(false);
#endif
    }

// 	Update is called once per frame
// 				void Update ()
// 			    {
// 					
// 				}
}