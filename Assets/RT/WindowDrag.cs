using UnityEngine;
using UnityEngine.EventSystems;

/*
Attach to any GUI object to allow it to be dragged around the screen with the mouse

by Seth A Robinson
*/

public class WindowDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    private bool _bDragging;

    private Vector2 _vStartDragPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        gameObject.transform.SetAsLastSibling();

        _vStartDragPos = eventData.position;
        _bDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_bDragging)
        {
            var vOffset = eventData.position - _vStartDragPos;
            _vStartDragPos = eventData.position;

            gameObject.transform.Translate(vOffset); //2d to 3d so z will be 0
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _bDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.transform.SetAsLastSibling();
    }
}