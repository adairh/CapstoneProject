
using UnityEngine;

namespace Manipulator
{
    public class PrebuiltDrawingHandler : MonoBehaviour
    {
        public static PrebuiltDrawingHandler Instance { get; private set; }

        private IPrebuiltDrawer currentDrawer;
        private bool isDrawing = false;
        private Vector3 startPos;

        private void Awake()
        {
            Instance = this;
        }

        public void StartDrawing(IPrebuiltDrawer drawer)
        {
            if (Input.GetMouseButtonDown(0))
            {
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

        private void Update()
        {
            if (!isDrawing || currentDrawer == null) return;

            if (Input.GetMouseButton(0))
            {
                if (PerformDrawing.RaycastMouse(out Vector3 pos))
                    currentDrawer.Working(pos);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (PerformDrawing.RaycastMouse(out Vector3 pos))
                    currentDrawer.End(pos);
                
                ManipulationManager.Instance.IsDrawing = false;
                PerformDrawing.ResetMode();
                isDrawing = false;
                currentDrawer = null;
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
