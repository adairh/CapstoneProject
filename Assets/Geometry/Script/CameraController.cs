// Ultra-functional camera controller using InputManager, with zoom fix, WASD, and shape tracking
using UnityEngine;

namespace Manipulator
{

    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Focus")] public Transform target;
        public Vector3 targetOffset = Vector3.zero;
        public float focusTransitionSpeed = 5f;

        [Header("Rotation")] public float rotationSpeed = 5f;
        public float minPitch = -85f;
        public float maxPitch = 85f;
        public bool invertY = false;

        [Header("Zoom")] public float zoomSpeed = 30f;
        public float minZoom = 2f;
        public float maxZoom = 80f;
        public bool zoomToCursor = false;

        [Header("Pan")] public float panSpeed = 0.3f;
        public float keyboardPanSpeed = 10f;

        [Header("Boundaries")] public bool useBounds = false;
        public Vector3 minBounds;
        public Vector3 maxBounds;

        [Header("Reset & States")] public Transform defaultTarget;
        public float defaultZoom = 10f;

        [Header("Debug")] public bool showDebugInfo = false;

        private float yaw;
        private float pitch = 30f;
        private float distance;

        private Vector3 lastMousePos;
        private Camera cam;

        private Vector3 targetVelocity;
        private Vector3 desiredTargetPos;
        private bool isTransitioningTarget = false;


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
                ClampCameraToBounds();
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

        private void UpdateCameraPosition()
        {
            if (isTransitioningTarget)
            {
                target.position = Vector3.SmoothDamp(target.position, desiredTargetPos, ref targetVelocity,
                    1f / focusTransitionSpeed);
                if ((target.position - desiredTargetPos).sqrMagnitude < 0.001f)
                    isTransitioningTarget = false;
            }
            else
            {
                target.position = desiredTargetPos;
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 offset = rotation * new Vector3(0, 0, -distance);
            transform.position = target.position + targetOffset + offset;
            transform.rotation = rotation;
        }

        private void ClampCameraToBounds()
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

        private void DebugDraw()
        {
            Debug.DrawLine(transform.position, target.position, Color.yellow);
            Debug.DrawRay(transform.position, transform.forward * 5, Color.cyan);
        }
    }
}
