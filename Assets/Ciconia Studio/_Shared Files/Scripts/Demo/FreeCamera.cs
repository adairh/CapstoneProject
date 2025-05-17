using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public bool enableInputCapture = true;
    public bool holdRightMouseCapture;

    public float lookSpeed = 5f;
    public float moveSpeed = 5f;
    public float sprintSpeed = 50f;

    private bool m_inputCaptured;
    private float m_pitch;
    private float m_yaw;

    private void Awake()
    {
        enabled = enableInputCapture;
    }

    private void Update()
    {
        if (!m_inputCaptured)
        {
            if (!holdRightMouseCapture && Input.GetMouseButtonDown(0))
                CaptureInput();
            else if (holdRightMouseCapture && Input.GetMouseButtonDown(1))
                CaptureInput();
        }

        if (!m_inputCaptured)
            return;

        if (m_inputCaptured)
        {
            if (!holdRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
                ReleaseInput();
            else if (holdRightMouseCapture && Input.GetMouseButtonUp(1))
                ReleaseInput();
        }

        var rotStrafe = Input.GetAxis("Mouse X");
        var rotFwd = Input.GetAxis("Mouse Y");

        m_yaw = (m_yaw + lookSpeed * rotStrafe) % 360f;
        m_pitch = (m_pitch - lookSpeed * rotFwd) % 360f;
        transform.rotation = Quaternion.AngleAxis(m_yaw, Vector3.up) * Quaternion.AngleAxis(m_pitch, Vector3.right);

        var speed = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
        var forward = speed * Input.GetAxis("Vertical");
        var right = speed * Input.GetAxis("Horizontal");
        var up = speed * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));
        transform.position += transform.forward * forward + transform.right * right + Vector3.up * up;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (m_inputCaptured && !focus)
            ReleaseInput();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            enabled = enableInputCapture;
    }

    private void CaptureInput()
    {
        m_inputCaptured = true;

        m_yaw = transform.eulerAngles.y;
        m_pitch = transform.eulerAngles.x;
    }

    private void ReleaseInput()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        m_inputCaptured = false;
    }
}