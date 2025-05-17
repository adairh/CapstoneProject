using UnityEditor;
using UnityEngine;

//allows right mouse to close the app, useful during development, probably not a great idea for your
//released product tho

public class QuitOnRightMouseButton : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Quitting app because right mouse button was pressed!");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}