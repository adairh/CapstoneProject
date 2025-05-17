using UnityEngine;

public class SetCanvasToActiveCamera : MonoBehaviour
{
    private void Awake()
    {
        // Start is called before the first frame update
        var s = gameObject.GetComponent<Canvas>();
        s.worldCamera = Camera.allCameras[0];
    }
}