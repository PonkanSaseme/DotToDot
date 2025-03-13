using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    private InputAction touchPress;     // 偵測按壓
    private InputAction touchPosition;  // 讀取滑鼠 / 觸控座標

    private Vector2 startTouchPos;
    private bool isDragging = false;

    public RectTransform ticketTransform;
    public float dragThreshold = 300f; // 觸發距離
    public System.Action OnDragComplete; // 拖曳完成時觸發

    private void Awake()
    {
        // 初始化 InputAction
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

        // 轉換螢幕座標到 UI 座標
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
            OnDragComplete?.Invoke(); // 通知 GachaSystem
        }
    }

    private void EndDrag()
    {
        isDragging = false;
    }
}
