using System.Collections.Generic;
using Manipulator;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    private readonly List<BaseButton> buttons = new();
    public static ButtonController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("ButtonController is initialized.");
    }

    public void RegisterButton(BaseButton button)
    {
        if (!buttons.Contains(button))
        {
            buttons.Add(button);
            Debug.Log($"Registered Button: {button.name}");
        }
    }

    public void OnButtonClicked(BaseButton button)
    {
        Debug.Log($"Button Clicked: {button.name}");
    }
    
    
    public void RequestUndo()
    {
        UndoRedoNetworkBridge.Instance.RequestUndoServerRpc();
    }
    
    public void RequestRedo()
    {
        UndoRedoNetworkBridge.Instance.RequestRedoServerRpc();
    }

    public void CameraReset()
    {
        CameraController.Instance.ResetCamera();
    }
}