using UnityEngine;
using UnityEngine.EventSystems;

namespace Manipulator
{
    public class PrebuiltDrawingHandler : MonoBehaviour
    {
        private IPrebuiltDrawer currentDrawer;
        private bool isDrawing;
        private Vector3 startPos;
        public static PrebuiltDrawingHandler Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!isDrawing || currentDrawer == null) return;

            if (Input.GetMouseButton(0))
            {
                if (PerformDrawing.RaycastMouse(out var pos))
                    currentDrawer.Working(pos);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (PerformDrawing.RaycastMouse(out var pos))
                    currentDrawer.End(pos);

                ManipulationManager.Instance.IsDrawing = false;
                PerformDrawing.ResetMode();
                isDrawing = false;
                currentDrawer = null;
            }
        }

        public void StartDrawing(IPrebuiltDrawer drawer)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                
                if (isDrawing || ManipulationManager.Instance.IsDrawing) return;

                Debug.LogError($"[prebuilt] {drawer}");

                currentDrawer = drawer;
                isDrawing = true;

                if (PerformDrawing.RaycastMouse(out startPos))
                {
                    drawer.Begin(startPos);
                    ManipulationManager.Instance.IsDrawing = true;
                }
            }
        }

        public void Cancel()
        {
            if (isDrawing && currentDrawer != null)
            {
                currentDrawer.Cancel();
                currentDrawer = null;
                isDrawing = false;
            }
        }
    }
}