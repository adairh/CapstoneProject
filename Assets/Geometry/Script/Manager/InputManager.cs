using System;
using UnityEngine;

namespace Manipulator
{
    public enum UserAction
    {
        Down,
        Up,
        Delete,
        Select,
        AngleCons,
        Config,
        OpenSettings,
        Draw,
        RightClick,
        CameraRotate,
        CameraPan,
        CameraZoomIn,
        CameraZoomOut,
        CameraReset,
        CameraMoveForward,
        CameraMoveBackward,
        CameraMoveLeft,
        CameraMoveRight,
        CameraMoveUp,
        CameraMoveDown
        // Other actions...
    }

    [DefaultExecutionOrder(-100)]
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
                OnAction?.Invoke(UserAction.Down, Input.mousePosition);

            if (Input.GetMouseButtonUp(0))
                OnAction?.Invoke(UserAction.Up, Input.mousePosition);

            if (Input.GetMouseButtonDown(1))
                OnAction?.Invoke(UserAction.Config, Input.mousePosition);

            if (Input.GetMouseButtonDown(1) && Input.GetKeyDown(KeyCode.LeftControl))
                OnAction?.Invoke(UserAction.Select, Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.A))
                OnAction?.Invoke(UserAction.AngleCons, Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.Delete))
                OnAction?.Invoke(UserAction.Delete, Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftAlt))
                SaveLoadManager.SaveAll();

            if (Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.LeftAlt))
                SaveLoadManager.LoadAll();

            if (Input.GetKeyDown(KeyCode.Z))
                UndoRedoNetworkBridge.Instance.RequestUndoServerRpc();

            if (Input.GetKeyDown(KeyCode.Y))
                UndoRedoNetworkBridge.Instance.RequestRedoServerRpc();
            
            if (Input.GetMouseButton(1))
                OnAction?.Invoke(UserAction.CameraRotate, Input.mousePosition);
            if (Input.GetMouseButton(2))
                OnAction?.Invoke(UserAction.CameraPan, Input.mousePosition);
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                OnAction?.Invoke(UserAction.CameraZoomIn, Input.mousePosition);
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
                OnAction?.Invoke(UserAction.CameraZoomOut, Input.mousePosition);
            if (Input.GetKeyDown(KeyCode.R))
                OnAction?.Invoke(UserAction.CameraReset, Input.mousePosition);
            if (Input.GetKey(KeyCode.W))
                OnAction?.Invoke(UserAction.CameraMoveForward, Input.mousePosition);
            if (Input.GetKey(KeyCode.A))
                OnAction?.Invoke(UserAction.CameraMoveLeft, Input.mousePosition);
            if (Input.GetKey(KeyCode.S))
                OnAction?.Invoke(UserAction.CameraMoveBackward, Input.mousePosition);
            if (Input.GetKey(KeyCode.D))
                OnAction?.Invoke(UserAction.CameraMoveRight, Input.mousePosition);

#elif UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                    OnAction?.Invoke(UserAction.Down, t.position);
                else if (t.phase == TouchPhase.Ended)
                    OnAction?.Invoke(UserAction.Up, t.position);
            }
#endif
        }

        public event Action<UserAction, Vector2> OnAction;
    }
}
