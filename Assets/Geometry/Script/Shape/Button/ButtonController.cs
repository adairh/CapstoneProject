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
        

        // --- CAMERA VISUAL CONTROLS ---

        public void CycleCameraFOV()
        {
            fovState = (fovState + 1) % fovPresets.Length;
            if (mainCamera != null) mainCamera.fieldOfView = fovPresets[fovState];
        }

        public void TogglePerspective()
        {
            if (mainCamera != null) mainCamera.orthographic = !mainCamera.orthographic;
        }

        public void CycleBackgroundColor()
        {
            bgColorState = (bgColorState + 1) % bgColorPresets.Length;
            if (mainCamera != null) mainCamera.backgroundColor = bgColorPresets[bgColorState];
        }

        public void CycleSkybox()
        {
            if (skyboxPresets == null || skyboxPresets.Count == 0) return;
            skyboxState = (skyboxState + 1) % skyboxPresets.Count;
            RenderSettings.skybox = skyboxPresets[skyboxState];
        }

        public void SnapCameraToTop()
        {
            if (cameraController != null)
            {
                cameraController.pitch = 90f;
                cameraController.yaw = 0f;
                cameraController.UpdateCameraPosition();
            }
        }
        public void SnapCameraToFront()
        {
            if (cameraController != null)
            {
                cameraController.pitch = 0f;
                cameraController.yaw = 0f;
                cameraController.UpdateCameraPosition();
            }
        }
        public void SnapCameraToSide()
        {
            if (cameraController != null)
            {
                cameraController.pitch = 0f;
                cameraController.yaw = 90f;
                cameraController.UpdateCameraPosition();
            }
        }
        public void SnapCameraToIso()
        {
            if (cameraController != null)
            {
                cameraController.pitch = 30f;
                cameraController.yaw = 45f;
                cameraController.UpdateCameraPosition();
            }
        }

        // --- LIGHT CONTROLS ---

        public void CycleLightIntensity()
        {
            if (sceneLight == null) return;
            lightIntensityState = (lightIntensityState + 1) % lightIntensityPresets.Length;
            sceneLight.intensity = lightIntensityPresets[lightIntensityState];
        }

        public void CycleLightColor()
        {
            if (sceneLight == null) return;
            lightColorState = (lightColorState + 1) % lightColorPresets.Length;
            sceneLight.color = lightColorPresets[lightColorState];
        }

        public void ToggleSceneLight()
        {
            if (sceneLight != null)
                sceneLight.enabled = !sceneLight.enabled;
        }

        public void ToggleLightShadows()
        {
            if (sceneLight != null)
                sceneLight.shadows = (sceneLight.shadows == LightShadows.None) ? LightShadows.Soft : LightShadows.None;
        }

        // --- POSTPROCESSING ---

        public void TogglePostProcessing()
        {
            if (postProcessVolume != null)
                postProcessVolume.enabled = !postProcessVolume.enabled;
        }

        // --- FOG CONTROLS ---

        public void ToggleFog()
        {
            RenderSettings.fog = !RenderSettings.fog;
        }

        public void CycleFogDensity()
        {
            fogDensityState = (fogDensityState + 1) % fogDensityPresets.Length;
            RenderSettings.fogDensity = fogDensityPresets[fogDensityState];
        }

        // --- AMBIENT CONTROLS ---

        public void SetAmbientLightWhite() => RenderSettings.ambientLight = Color.white;
        public void SetAmbientLightWarm() => RenderSettings.ambientLight = new Color(1f, 0.95f, 0.8f);
        public void SetAmbientLightCool() => RenderSettings.ambientLight = new Color(0.7f, 0.85f, 1f);

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
