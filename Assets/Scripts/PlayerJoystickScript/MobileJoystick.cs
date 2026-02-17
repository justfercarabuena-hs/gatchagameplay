using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform handle;
    public float radius = 100f;

    Vector2 inputVector;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        pos = Vector2.ClampMagnitude(pos, radius);
        handle.localPosition = pos;
        inputVector = pos / radius;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.localPosition = Vector2.zero;
        inputVector = Vector2.zero;
    }
}
