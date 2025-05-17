using UnityEngine;
using UnityEngine.InputSystem;

public class ConsoleToggle : MonoBehaviour
{
    private string _oldText;

    // Update is called once per frame
    private void Update()
    {
        //    if (!RTUtil.IsHeadless())
        if (Keyboard.current != null)
            if (Keyboard.current.backquoteKey.wasPressedThisFrame && Keyboard.current.shiftKey.isPressed)
                ToggleConsole();
    }

    public void ToggleConsole()
    {
        //print("Toggling debug console");

        var debugCanvas = RTConsole.Get().transform.parent.gameObject;

        debugCanvas.SetActive(!debugCanvas.activeSelf);
        if (debugCanvas.activeSelf)
            RTConsole.Get().SetFocusOnInput(_oldText);
        else
            //save what was there
            _oldText = RTConsole.Get().GetCurrentText().Replace("~", "");
        //return debugCanvas.activeSelf;
    }
}