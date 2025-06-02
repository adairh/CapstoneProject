using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // For Volume (URP/HDRP postprocessing)

namespace Manipulator {
    public class ButtonController : MonoBehaviour
    {
        
        private readonly List<BaseButton> buttons = new();
        
        // Camera and Light references, assign in Inspector
        public CameraController cameraController;
        public Camera mainCamera;
        public Light sceneLight;

        // Skybox and visual presets
        public List<Material> skyboxPresets; // Drag skybox materials here in Inspector
        public Volume postProcessVolume;     // For URP/HDRP, else leave null

        // --- Presets for cycling ---
        private int fovState = 0;
        private readonly float[] fovPresets = { 30f, 60f, 90f };

        private int bgColorState = 0;
        private readonly Color[] bgColorPresets = {
            Color.white, new Color(0.9f,0.9f,0.9f), Color.gray, Color.black, new Color(0.7f,0.9f,1f)
        };

        private int skyboxState = 0;

        private int lightIntensityState = 0;
        private readonly float[] lightIntensityPresets = { 0.5f, 1f, 2f };

        private int lightColorState = 0;
        private readonly Color[] lightColorPresets = {
            Color.white, new Color(1f, 0.95f, 0.8f), new Color(0.7f, 0.85f, 1f), Color.red, Color.green
        };

        private int fogDensityState = 0;
        private readonly float[] fogDensityPresets = { 0f, 0.01f, 0.025f, 0.05f };

        // --- Singleton pattern ---
        public static ButtonController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
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
        
        // Add this function!
        public void ResetAllToggles()
        {
            foreach (var btn in buttons)
            {
                if (btn != null && btn.IsToggleButton)
                    btn.ResetButton();
            }
        }

        public void OnFrontViewButton() => cameraController.SnapToFront();
        public void OnTopViewButton() => cameraController.SnapToTop();
        public void OnSideViewButton() => cameraController.SnapToSide();
        public void OnIsoViewButton() => cameraController.SnapToIso();

        public void OnTogglePerspective() => cameraController.TogglePerspective();
        public void OnCycleFOV() => cameraController.CycleCameraFOV();
        public void OnCycleBackground() => cameraController.CycleBackgroundColor();
        public void OnCycleSkybox() => cameraController.CycleSkybox();
// ...and so on for your other controls!

        
        // --- EXISTING BUTTONS (Undo, Redo, etc.) ---

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
            if (cameraController != null)
                cameraController.ResetCamera();
        }

        public void SetAxisLockMode(int mode)
        {
            Manipulator.ManipulationManager.Instance.CurrentAxisLock = (Manipulator.AxisLockMode)mode;
        }
    }
}
