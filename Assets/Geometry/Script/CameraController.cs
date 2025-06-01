using UnityEngine;
using System.Collections;

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

        public float yaw;
        public float pitch = 30f;
        private float distance;

        private Vector3 lastMousePos;
        private Camera cam;

        private Vector3 targetVelocity;
        private Vector3 desiredTargetPos;
        private bool isTransitioningTarget = false;

        // Smooth transition
        private Coroutine zoneTransitionRoutine;

        public static CameraController Instance;

        private void Start()
        {
            cam = GetComponent<Camera>();

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

            if (InputManager.Instance != null)
                InputManager.Instance.OnAction += HandleCameraInput;

            Instance = this;
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnAction -= HandleCameraInput;
        }

        private void LateUpdate()
        {
            UpdateCameraPosition();
            if (useBounds)
                ClampTargetToBounds();
            if (showDebugInfo)
                DebugDraw();
        }

        private void HandleCameraInput(UserAction action, Vector2 screenPos)
        {
            switch (action)
            {
                case UserAction.CameraRotate:
                    Vector3 delta = Input.mousePosition - lastMousePos;
                    yaw += delta.x * rotationSpeed * Time.deltaTime;
                    pitch += (invertY ? delta.y : -delta.y) * rotationSpeed * Time.deltaTime;
                    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                    break;

                case UserAction.CameraPan:
                    Vector3 panDelta = Input.mousePosition - lastMousePos;
                    Vector3 move = -transform.right * panDelta.x - transform.up * panDelta.y;
                    desiredTargetPos += move * panSpeed * Time.deltaTime;
                    break;

                case UserAction.CameraZoomIn:
                    distance -= zoomSpeed * Time.deltaTime;
                    distance = Mathf.Clamp(distance, minZoom, maxZoom);
                    break;

                case UserAction.CameraZoomOut:
                    distance += zoomSpeed * Time.deltaTime;
                    distance = Mathf.Clamp(distance, minZoom, maxZoom);
                    break;

                case UserAction.CameraReset:
                    ResetCamera();
                    break;

                case UserAction.CameraMoveForward:
                    desiredTargetPos += transform.forward * keyboardPanSpeed * Time.deltaTime;
                    break;

                case UserAction.CameraMoveBackward:
                    desiredTargetPos -= transform.forward * keyboardPanSpeed * Time.deltaTime;
                    break;

                case UserAction.CameraMoveLeft:
                    desiredTargetPos -= transform.right * keyboardPanSpeed * Time.deltaTime;
                    break;

                case UserAction.CameraMoveRight:
                    desiredTargetPos += transform.right * keyboardPanSpeed * Time.deltaTime;
                    break;
            }

            lastMousePos = Input.mousePosition;
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

                // SphereCast from target towards the desired camera position
                if (Physics.SphereCast(target.position, cameraCollisionRadius, dir, out RaycastHit hit, maxDist, zoneBoundaryLayerMask))
                {
                    // Move camera as close as possible to the hit point (minus offset)
                    float hitDist = hit.distance - cameraMinDistanceToBoundary;
                    hitDist = Mathf.Clamp(hitDist, minZoom, maxDist); // Prevent going inside target or negative!
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
        }

        public void FocusOnTarget(Transform newTarget)
        {
            target = newTarget;
            desiredTargetPos = newTarget.position;
            isTransitioningTarget = true;
        }

        // --- NEW: Smooth zone transitions ---
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
            float targetDistance = defaultZoom; // Or keep previous zoom if preferred

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
    }
}
