using UnityEngine;
using UnityEngine.EventSystems;

public class DisableScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Блокируем начало перетаскивания
        eventData.pointerDrag = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Блокируем перетаскивание
        eventData.pointerDrag = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Блокируем завершение перетаскивания
        eventData.pointerDrag = null;
    }
}