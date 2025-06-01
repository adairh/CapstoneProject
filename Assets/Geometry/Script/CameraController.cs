using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Manipulator
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Focus")]
        public Transform target;
        public Vector3 targetOffset = Vector3.zero;
        public float focusTransitionSpeed = 5f;

        [Header("Rotation")]
        public float rotationSpeed = 5f;
        public float minPitch = -85f;
        public float maxPitch = 85f;
        public bool invertY = false;

        [Header("Zoom")]
        public float zoomSpeed = 30f;
        public float minZoom = 2f;
        public float maxZoom = 80f;
        public bool zoomToCursor = false;

        [Header("Pan")]
        public float panSpeed = 0.3f;
        public float keyboardPanSpeed = 10f;

        [Header("Boundaries")]
        public bool useBounds = false;
        public Vector3 minBounds;
        public Vector3 maxBounds;

        [Header("Zone Collision")]
        public LayerMask zoneBoundaryLayerMask;
        public float cameraCollisionRadius = 0.2f;
        public float cameraMinDistanceToBoundary = 0.5f;

        [Header("Reset & States")]
        public Transform defaultTarget;
        public float defaultZoom = 10f;

        [Header("Debug")]
        public bool showDebugInfo = false;

        [Header("Projection/FOV Presets")]
        public float[] fovPresets = { 60f, 45f, 75f };
        public Color[] bgColorPresets;
        public List<Material> skyboxPresets;

        public int fovState = 0;
        public int bgColorState = 0;
        public int skyboxState = 0;

        public float yaw;
        public float pitch = 30f;
        private float distance;
        private Camera cam;

        private Vector3 lastMousePos;
        private Vector3 targetVelocity;
        private Vector3 desiredTargetPos;
        private bool isTransitioningTarget = false;
        private bool isDragging = false;

        // Smooth transition
        private Coroutine zoneTransitionRoutine;

        public static CameraController Instance;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
                cam = Camera.main;
            Instance = this;
        }

        private void Start()
        {
            if (target == null && defaultTarget != null)
                target = Instantiate(defaultTarget);

            if (target == null)
            {
                GameObject t = new GameObject("Camera Target");
                t.transform.position = Vector3.zero;
                target = t.transform;
            }

            desiredTargetPos = target.position;
            distance = defaultZoom;

            UpdateCameraPosition();
        }

        private void LateUpdate()
        {
            HandleInput();
            UpdateCameraPosition();
            if (useBounds)
                ClampTargetToBounds();
            if (showDebugInfo)
                DebugDraw();
        }

        private void HandleInput()
        {
            // --- Orbit with right mouse ---
            if (Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(1))
                isDragging = false;

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                yaw += delta.x * rotationSpeed * 0.1f;
                pitch += (invertY ? delta.y : -delta.y) * rotationSpeed * 0.1f;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                lastMousePos = Input.mousePosition;
            }

            // --- Zoom with scroll wheel ---
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                cam.orthographic = false;
                distance -= scroll * zoomSpeed;
                distance = Mathf.Clamp(distance, minZoom, maxZoom);
            }

            // --- Pan with middle mouse ---
            if (Input.GetMouseButtonDown(2))
            {
                lastMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButton(2))
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                Vector3 move = -transform.right * delta.x - transform.up * delta.y;
                desiredTargetPos += move * panSpeed * 0.01f;
                lastMousePos = Input.mousePosition;
            }

            // --- Pan with keyboard ---
            float panH = Input.GetAxis("Horizontal");
            float panV = Input.GetAxis("Vertical");
            if (Mathf.Abs(panH) > 0.01f || Mathf.Abs(panV) > 0.01f)
            {
                desiredTargetPos += (transform.right * panH + transform.up * panV) * keyboardPanSpeed * Time.deltaTime;
            }
        }

        public void UpdateCameraPosition()
        {
            if (isTransitioningTarget)
            {
                target.position = Vector3.SmoothDamp(target.position, desiredTargetPos, ref targetVelocity, 1f / focusTransitionSpeed);
                if ((target.position - desiredTargetPos).sqrMagnitude < 0.001f)
                    isTransitioningTarget = false;
            }
            else
            {
                target.position = desiredTargetPos;
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredOffset = rotation * new Vector3(0, 0, -distance);
            Vector3 desiredCamPos = target.position + targetOffset + desiredOffset;

            // --- Enhanced Camera Collision (Pull Back Effect) ---
            Vector3 finalCamPos = desiredCamPos;
            if (zoneBoundaryLayerMask != 0)
            {
                Vector3 dir = (desiredCamPos - target.position).normalized;
                float maxDist = Vector3.Distance(target.position, desiredCamPos);

                if (Physics.SphereCast(target.position, cameraCollisionRadius, dir, out RaycastHit hit, maxDist, zoneBoundaryLayerMask))
                {
                    float hitDist = hit.distance - cameraMinDistanceToBoundary;
                    hitDist = Mathf.Clamp(hitDist, minZoom, maxDist);
                    finalCamPos = target.position + dir * hitDist;
                }
            }

            transform.position = finalCamPos;
            transform.rotation = rotation;
        }

        private void ClampTargetToBounds()
        {
            Vector3 clamped = target.position;
            clamped.x = Mathf.Clamp(clamped.x, minBounds.x, maxBounds.x);
            clamped.y = Mathf.Clamp(clamped.y, minBounds.y, maxBounds.y);
            clamped.z = Mathf.Clamp(clamped.z, minBounds.z, maxBounds.z);
            target.position = clamped;
            desiredTargetPos = clamped;
        }

        public void ResetCamera()
        {
            pitch = 30f;
            yaw = 0f;
            distance = defaultZoom;
            desiredTargetPos = defaultTarget != null ? defaultTarget.position : Vector3.zero;
            isTransitioningTarget = true;
            lastMousePos = Input.mousePosition; // Reset after snap
        }

        public void FocusOnTarget(Transform newTarget)
        {
            target = newTarget;
            desiredTargetPos = newTarget.position;
            isTransitioningTarget = true;
            lastMousePos = Input.mousePosition;
        }

        // --- CAMERA PROJECTION SNAPS ---

        public void SnapToFront()
        {
            pitch = 0f;
            yaw = 0f;
            cam.orthographic = true;
            UpdateCameraPosition();
            lastMousePos = Input.mousePosition;
        }

        public void SnapToTop()
        {
            pitch = 90f;
            yaw = 0f;
            cam.orthographic = true;
            UpdateCameraPosition();
            lastMousePos = Input.mousePosition;
        }

        public void SnapToSide()
        {
            pitch = 0f;
            yaw = 90f;
            cam.orthographic = true;
            UpdateCameraPosition();
            lastMousePos = Input.mousePosition;
        }

        public void SnapToIso()
        {
            pitch = 30f;
            yaw = 45f;
            cam.orthographic = false;
            UpdateCameraPosition();
            lastMousePos = Input.mousePosition;
        }

        // --- CAMERA VISUAL CONTROLS ---

        public void CycleCameraFOV()
        {
            fovState = (fovState + 1) % fovPresets.Length;
            if (cam != null) cam.fieldOfView = fovPresets[fovState];
        }

        public void TogglePerspective()
        {
            if (cam != null) cam.orthographic = !cam.orthographic;
        }

        public void CycleBackgroundColor()
        {
            bgColorState = (bgColorState + 1) % bgColorPresets.Length;
            if (cam != null) cam.backgroundColor = bgColorPresets[bgColorState];
        }

        public void CycleSkybox()
        {
            if (skyboxPresets == null || skyboxPresets.Count == 0) return;
            skyboxState = (skyboxState + 1) % skyboxPresets.Count;
            RenderSettings.skybox = skyboxPresets[skyboxState];
        }
// --- SMOOTH ZONE TRANSITION ---

        public void MoveToZoneSmooth(Vector3 zoneCenter, float duration = 1.0f)
        {
            if (zoneTransitionRoutine != null)
                StopCoroutine(zoneTransitionRoutine);
            zoneTransitionRoutine = StartCoroutine(SmoothTransitionToZone(zoneCenter, duration));
        }

        private IEnumerator SmoothTransitionToZone(Vector3 newTargetPos, float duration)
        {
            Vector3 startTargetPos = target.position;
            float startDistance = distance;
            float elapsed = 0f;
            float targetDistance = defaultZoom;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                target.position = Vector3.Lerp(startTargetPos, newTargetPos, t);
                desiredTargetPos = target.position;
                distance = Mathf.Lerp(startDistance, targetDistance, t);
                yield return null;
            }
            target.position = newTargetPos;
            desiredTargetPos = newTargetPos;
            distance = targetDistance;
            isTransitioningTarget = false;
        }

        private void DebugDraw()
        {
            Debug.DrawLine(transform.position, target.position, Color.yellow);
            Debug.DrawRay(transform.position, transform.forward * 5, Color.cyan);
        }

        // You can add your light, ambient, fog, post-processing, etc controls here as needed!
    }
}
