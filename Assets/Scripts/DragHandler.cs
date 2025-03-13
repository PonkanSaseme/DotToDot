using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    private InputAction touchPress;     // ��������
    private InputAction touchPosition;  // Ū���ƹ� / Ĳ���y��

    private Vector2 startTouchPos;
    private bool isDragging = false;

    public RectTransform ticketTransform;
    public float dragThreshold = 300f; // Ĳ�o�Z��
    public System.Action OnDragComplete; // �즲������Ĳ�o

    private void Awake()
    {
        // ��l�� InputAction
        touchPress = new InputAction("TouchPress", InputActionType.Button, "<Pointer>/press");
        touchPosition = new InputAction("TouchPosition", InputActionType.Value, "<Pointer>/position");
    }

    private void OnEnable()
    {
        touchPress.Enable();
        touchPosition.Enable();
        touchPress.started += ctx => StartDrag();
        touchPress.canceled += ctx => EndDrag();
    }

    private void OnDisable()
    {
        touchPress.started -= ctx => StartDrag();
        touchPress.canceled -= ctx => EndDrag();
        touchPress.Disable();
        touchPosition.Disable();
    }

    private void StartDrag()
    {
        isDragging = true;
        Vector2 touchScreenPos = touchPosition.ReadValue<Vector2>();

        // �ഫ�ù��y�Ш� UI �y��
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ticketTransform.parent as RectTransform,
            touchScreenPos,
            null,
            out startTouchPos
        );
    }

    private void Update()
    {
        if (!isDragging) return;

        Vector2 touchScreenPos = touchPosition.ReadValue<Vector2>();

        Vector2 currentTouchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ticketTransform.parent as RectTransform,
            touchScreenPos,
            null,
            out currentTouchPos
        );

        float deltaY = currentTouchPos.y - startTouchPos.y;
        Vector2 newPos = startTouchPos + new Vector2(0, deltaY);
        ticketTransform.anchoredPosition = newPos;

        if (ticketTransform.anchoredPosition.y >= startTouchPos.y + dragThreshold)
        {
            isDragging = false;
            OnDragComplete?.Invoke(); // �q�� GachaSystem
        }
    }

    private void EndDrag()
    {
        isDragging = false;
    }
}
