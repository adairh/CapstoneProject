using System;
using UnityEngine;

namespace Manipulator
{
    public enum UserAction
    {
        LeftClick,
        RightClick,
        Drag,
        Draw,
        OpenSettings,
        Select,
        AngleCons,
        Config,

        Delete
        // … thêm tùy bạn
    }

    [DefaultExecutionOrder(-100)]
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

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

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
                OnAction?.Invoke(UserAction.LeftClick, Input.mousePosition);

            if (Input.GetMouseButtonDown(1))
                OnAction?.Invoke(UserAction.Config, Input.mousePosition);

            if (Input.GetMouseButton(0))
                OnAction?.Invoke(UserAction.Drag, Input.mousePosition);

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


#elif UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                    OnAction?.Invoke(UserAction.LeftClick, t.position);
                else if (t.phase == TouchPhase.Moved)
                    OnAction?.Invoke(UserAction.Drag, t.position);
            }
#endif
        }

        // Sự kiện chung, truyền kèm action và vị trí
        public event Action<UserAction, Vector2> OnAction;
    }
}