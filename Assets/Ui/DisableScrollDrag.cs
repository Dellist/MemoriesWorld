using UnityEngine;
using UnityEngine.EventSystems;

public class DisableScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        // ��������� ������ ��������������
        eventData.pointerDrag = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ��������� ��������������
        eventData.pointerDrag = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ��������� ���������� ��������������
        eventData.pointerDrag = null;
    }
}