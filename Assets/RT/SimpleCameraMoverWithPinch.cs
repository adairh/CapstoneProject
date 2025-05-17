using UnityEngine;
using UnityEngine.EventSystems;

//this class handles pan and zoom controls for desktops + webgl.  I'm still using the older input
//controls for the mouse wheel input as I couldn't get that to work with the 1.3 new input system.

//To add/change controls, you should double click the InputActions.inputactions file and use Unity's
//system to make changes there.

//Seth A. Robinson 12/07/2022

public class SimpleCameraMoverWithPinch : MonoBehaviour
{
    public bool m_bReversePanDirection = true;

    // Start is called before the first frame update
    private InputActions controls;
    private bool m_bFirstFingerActive;
    private bool m_bPinchActive;

    private Camera m_curCam;
    private readonly float m_maxPerspectiveZoom = 2;
    private readonly float m_minPerspectiveZoom = -80;
    private Vector2 m_vCurPos1, m_vCurPos2;

    private Vector2 m_vStartPos1, m_vStartPos2;

    private void Awake()
    {
        controls = new InputActions();
        m_curCam = GetComponent<Camera>();
    }


    private void Start()
    {
        controls.Touch.SecondaryTouchContact.started += _ => SecondFingerStart();
        controls.Touch.SecondaryTouchContact.canceled += _ => SecondFingerEnd();
        controls.Touch.PrimaryTouchContact.started += _ => FirstFingerStart();
        controls.Touch.PrimaryTouchContact.canceled += _ => FirstFingerEnd();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!RTUtil.IsMouseOverGameWindow) return;

        //ignore clicks if we're over a GUI element
        var bOverGUI = EventSystem.current.IsPointerOverGameObject();

        var zoomSpeed = 0.05f;

        if (m_bPinchActive)
        {
            m_vCurPos1 = controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
            m_vCurPos2 = controls.Touch.SecondFingerPosition.ReadValue<Vector2>();
            var startDist = (m_vStartPos1 - m_vStartPos2).magnitude;

            var curDist = (m_vCurPos1 - m_vCurPos2).magnitude;
            var pinchOffset = curDist - startDist;

            // RTConsole.Log("Final pinch offset: "+pinchOffset);

            if (pinchOffset != 1)
            {
                var vPos = m_curCam.transform.position;
                vPos.z += pinchOffset * zoomSpeed;
                //force it to stay within zoom bounds
                if (vPos.z < m_minPerspectiveZoom || vPos.z > m_maxPerspectiveZoom)
                    vPos.z = Mathf.Clamp(vPos.z, m_minPerspectiveZoom, m_maxPerspectiveZoom);
                if (!bOverGUI) m_curCam.transform.position = vPos;

                //assume we applied it so we can reset the position
                m_vStartPos1 = controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
                m_vStartPos2 = controls.Touch.SecondFingerPosition.ReadValue<Vector2>();
            }

            return;
        }


        if (m_bFirstFingerActive)
        {
            //normal panning
            m_vCurPos1 = controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
            var vOffset = m_vCurPos1 - m_vStartPos1;
            if (vOffset.magnitude >= 0.01f)
            {
                //RTConsole.Log("Pan: " + vOffset);

                var WorldPosA =
                    m_curCam.ScreenToWorldPoint(new Vector3(m_vStartPos1.x, m_vStartPos1.y, transform.position.z));
                var WorldPosB =
                    m_curCam.ScreenToWorldPoint(new Vector3(m_vCurPos1.x, m_vCurPos1.y, transform.position.z));

                var vDisplacement = WorldPosB - WorldPosA;
                if (m_bReversePanDirection) vDisplacement *= -1;

                if (!bOverGUI)
                {
                    var vPos = m_curCam.transform.position;
                    vPos.x += vDisplacement.x;
                    vPos.y += vDisplacement.y;
                    m_curCam.transform.position = vPos;
                }

                //assume we applied it so we can reset the position
                m_vStartPos1 = controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
            }
        }

        if (m_curCam.orthographic)
            ProcessScrollWheelOrthographic();
        else
            ProcessScrollWheelPerspective();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    //also handle disabling
    private void OnDisable()
    {
        controls.Disable();
    }

    private void FirstFingerStart()
    {
        m_bFirstFingerActive = true;
        m_vStartPos1 = controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
    }

    //now the end
    private void FirstFingerEnd()
    {
        m_bFirstFingerActive = false;
    }

    private void SecondFingerEnd()
    {
        m_bPinchActive = false;
    }

    private void SecondFingerStart()
    {
        m_bPinchActive = true;
        m_vStartPos2 = controls.Touch.SecondFingerPosition.ReadValue<Vector2>();
    }

    private void ProcessScrollWheelOrthographic()
    {
        var minZoom = 0.3f;
        float maxZoom = 150;

        //Read the mousewheel and zoom in/out
        var wheel = Input.mouseScrollDelta.y;
        if (wheel != 0)
        {
            //RTConsole.Log("Mouse wheel: " + wheel);
            var size = m_curCam.orthographicSize;
            var mouseWheelZoomSpeed = Mathf.Clamp(Mathf.Abs(size * 0.08f), 0.5f, Mathf.Abs(size * 0.1f));
            size -= wheel * mouseWheelZoomSpeed;

            //force it to stay within zoom bounds
            if (size < minZoom || size > maxZoom) size = Mathf.Clamp(size, minZoom, maxZoom);

            m_curCam.orthographicSize = size;
        }
    }

    private void ProcessScrollWheelPerspective()
    {
        //Read the mousewheel and zoom in/out
        var wheel = Input.mouseScrollDelta.y;
        if (wheel != 0)
        {
            //RTConsole.Log("Mouse wheel: " + wheel);

            //move in bigger increments when zoomed out
            var vPos = m_curCam.transform.position;
            var mouseWheelZoomSpeed = Mathf.Clamp(Mathf.Abs(vPos.z * 0.08f), 0.5f, Mathf.Abs(vPos.z * 0.1f));

            vPos.z += wheel * mouseWheelZoomSpeed;
            //force it to stay within zoom bounds
            if (vPos.z < m_minPerspectiveZoom || vPos.z > m_maxPerspectiveZoom)
                vPos.z = Mathf.Clamp(vPos.z, m_minPerspectiveZoom, m_maxPerspectiveZoom);

            m_curCam.transform.position = vPos;
        }
    }
}