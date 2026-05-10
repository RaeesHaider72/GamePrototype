using UnityEngine;
using UnityEngine.EventSystems;

public class MobileTrackpad : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public Vector2 LookDelta { get; private set; }

    private Vector2 previousPosition;

    public float sensitivity = 0.15f;

    public void OnPointerDown(PointerEventData eventData)
    {
        previousPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPosition = eventData.position;

        LookDelta = (currentPosition - previousPosition) * sensitivity;

        previousPosition = currentPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        LookDelta = Vector2.zero;
    }

    private void LateUpdate()
    {
        LookDelta = Vector2.zero;
    }
}