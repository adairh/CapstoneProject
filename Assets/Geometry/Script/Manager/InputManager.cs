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
        Select
        // … thêm tùy bạn
    }

    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        // Sự kiện chung, truyền kèm action và vị trí
        public event Action<UserAction, Vector2> OnAction;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
                OnAction?.Invoke(UserAction.LeftClick, Input.mousePosition);
            if (Input.GetMouseButtonDown(1))
                OnAction?.Invoke(UserAction.RightClick, Input.mousePosition);
            if (Input.GetMouseButton(0))
                OnAction?.Invoke(UserAction.Drag, Input.mousePosition);
            
            
            if (Input.GetMouseButtonDown(1) && Input.GetKeyDown(KeyCode.LeftControl))
                OnAction?.Invoke(UserAction.Select, Input.mousePosition);
            
            
            
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                OnAction?.Invoke(UserAction.LeftClick, t.position);
            else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                OnAction?.Invoke(UserAction.Drag, t.position);
            else if (t.phase == TouchPhase.Ended)
                OnAction?.Invoke(UserAction.Draw, t.position);
        }
#endif
            

            // Ví dụ phím mở setting
            if (Input.GetKeyDown(KeyCode.Escape))
                OnAction?.Invoke(UserAction.OpenSettings, Vector2.zero);
        }
    }
}