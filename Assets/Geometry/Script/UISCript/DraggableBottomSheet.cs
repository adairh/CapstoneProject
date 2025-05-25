using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class DraggableBottomSheet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Optional: Assign a custom handle area (RectTransform).")]
    public RectTransform handleArea; // If null, tries to auto-find.

    [Header("Snap Animation")]
    public float snapSpeed = 10f;

    [Tooltip("Minimum height of drag handle area in pixels for easy interaction.")]
    public float minHandleRaycastHeight = 40f;

    private RectTransform panel;
    private Canvas rootCanvas;
    private bool dragging = false;
    private Vector2 dragStartPointerPos;
    private float dragStartPanelY;
    private Coroutine snapCoroutine;

    private float openedY; // Calculated!
    private float closedY; // Calculated!

    private float panelHeight;
    private float handleHeight;

    // Track which state we're in for resizing safety
    private bool isClosed = true;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (!handleArea)
            handleArea = AutoFindHandleArea();
        EnsureRaycastArea();
        LayoutRebuild();

        SetClosedImmediate();

        // If handle has a Button, add toggle functionality
        var button = handleArea ? handleArea.GetComponent<Button>() : null;
        if (button)
            button.onClick.AddListener(ToggleSheet);
    }

    // Handles screen/canvas/panel/handle resize
    void OnRectTransformDimensionsChange()
    {
        LayoutRebuild();

        // Snap to current state (open/closed) if not dragging/animating
        if (!dragging && snapCoroutine == null)
        {
            if (isClosed)
                SetClosedImmediate();
            else
                SetOpenedImmediate();
        }
    }

    void LayoutRebuild()
    {
        // float parentHeight = ((RectTransform)panel.parent).rect.height;
        // panelHeight = panel.rect.height;
        // handleHeight = handleArea ? handleArea.rect.height : minHandleRaycastHeight;

        openedY = -500;
        closedY = -1000;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsPointerOnHandle(eventData))
        {
            dragging = false;
            return;
        }
        dragging = true;
        dragStartPointerPos = eventData.position;
        dragStartPanelY = panel.anchoredPosition.y;
        if (snapCoroutine != null) StopCoroutine(snapCoroutine);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        float pointerDeltaY = eventData.position.y - dragStartPointerPos.y;
        float targetY = Mathf.Clamp(dragStartPanelY + pointerDeltaY, closedY, openedY);
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, targetY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        float currentY = panel.anchoredPosition.y;
        float thresholdY = closedY + (openedY - closedY) * 0.5f;
        float targetY = (currentY > thresholdY) ? openedY : closedY;
        isClosed = targetY == closedY;
        if (snapCoroutine != null) StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SmoothSnap(targetY));
    }

    IEnumerator SmoothSnap(float targetY)
    {
        float threshold = 1f;
        while (Mathf.Abs(panel.anchoredPosition.y - targetY) > threshold)
        {
            float newY = Mathf.Lerp(panel.anchoredPosition.y, targetY, Time.deltaTime * snapSpeed);
            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, newY);
            yield return null;
        }
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, targetY);
    }

    public void SetClosedImmediate()
    {
        LayoutRebuild();
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, closedY);
        isClosed = true;
    }

    public void SetOpenedImmediate()
    {
        LayoutRebuild();
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, openedY);
        isClosed = false;
    }

    public void OpenSheet()
    {
        LayoutRebuild();
        if (snapCoroutine != null) StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SmoothSnap(openedY));
        isClosed = false;
    }

    public void CloseSheet()
    {
        LayoutRebuild();
        if (snapCoroutine != null) StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SmoothSnap(closedY));
        isClosed = true;
    }

    private void ToggleSheet()
    {
        LayoutRebuild();
        float currentY = panel.anchoredPosition.y;
        bool isOpen = Mathf.Abs(currentY - openedY) < 1f;
        if (isOpen)
            CloseSheet();
        else
            OpenSheet();
    }

    // --- Helper Functions ---

    // Finds a child called "Handle" or with "handle" in the name, or with Image, or lowest child.
    RectTransform AutoFindHandleArea()
    {
        RectTransform found = null;
        float minY = float.MaxValue;
        foreach (RectTransform child in transform)
        {
            string name = child.name.ToLower();
            if (name.Contains("handle")) return child;
            if (!found && child.GetComponent<Image>()) found = child;
            if (child.anchoredPosition.y < minY)
            {
                minY = child.anchoredPosition.y;
                found = child;
            }
        }
        return found;
    }

    // Ensures handle is raycastable for easier touch/click
    void EnsureRaycastArea()
    {
        if (!handleArea) return;
        if (handleArea.rect.height < minHandleRaycastHeight)
        {
            var rt = handleArea;
            float diff = minHandleRaycastHeight - rt.rect.height;
            rt.offsetMin = new Vector2(rt.offsetMin.x, rt.offsetMin.y - diff);
        }
        if (!handleArea.GetComponent<Graphic>())
        {
            var img = handleArea.gameObject.AddComponent<Image>();
            img.color = new Color(0,0,0,0);
            img.raycastTarget = true;
        }
        else
        {
            var g = handleArea.GetComponent<Graphic>();
            g.raycastTarget = true;
        }
    }

    // Checks if pointer/touch is within handle area
    bool IsPointerOnHandle(PointerEventData eventData)
    {
        if (!handleArea) return true;
        return RectTransformUtility.RectangleContainsScreenPoint(handleArea, eventData.position, eventData.pressEventCamera);
    }

    // Helper to get world-space rect
    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        float x = corners[0].x;
        float y = corners[0].y;
        float width = corners[2].x - corners[0].x;
        float height = corners[2].y - corners[0].y;
        return new Rect(x, y, width, height);
    }
}
