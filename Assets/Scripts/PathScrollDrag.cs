using UnityEngine;
using UnityEngine.EventSystems;

public class PathScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform content;
    public RectTransform viewport;

    private Vector2 dragStartContentPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartContentPos = content.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float newX = dragStartContentPos.x + (eventData.position.x - eventData.pressPosition.x);
        SetContentX(newX);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void SetContentX(float x)
    {
        float minX = Mathf.Min(0f, viewport.rect.width - content.rect.width);
        x = Mathf.Clamp(x, minX, 0f);
        Vector2 pos = content.anchoredPosition;
        pos.x = x;
        content.anchoredPosition = pos;
    }
}
