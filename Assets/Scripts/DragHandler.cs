using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    private InputAction touchPress;     // 偵測按壓
    private InputAction touchPosition;  // 讀取滑鼠 / 觸控座標

    private Vector2 startTouchPos;
    private bool isDragging = false;

    public RectTransform ticketTransform;
    public float dragThreshold = 50f; // 觸發距離
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
        touchPress.performed += ctx => Dragging(); // 修正：加入拖曳事件
        touchPress.canceled += ctx => EndDrag();
    }

    private void OnDisable()
    {
        touchPress.started -= ctx => StartDrag();
        touchPress.canceled -= ctx => EndDrag();
        touchPress.Disable();
        touchPosition.Disable();
    }

    private void StartDrag() //開始拖曳(按住)
    {
        isDragging = true;
        Vector2 touchScreenPos = touchPosition.ReadValue<Vector2>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ticketTransform.parent as RectTransform,
            touchScreenPos,
            Camera.main,
            out startTouchPos
        );
    }

    private void Update() //持續偵測
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
        Vector2 newPos = ticketTransform.anchoredPosition;
        newPos.y += deltaY; // 只修改 Y 軸，確保拖曳順暢
        ticketTransform.anchoredPosition = newPos;


        if (ticketTransform.anchoredPosition.y >= startTouchPos.y + dragThreshold)
        {
            isDragging = false;
            OnDragComplete?.Invoke(); // 通知 GachaSystem
        }
    }

    private void Dragging() //拖曳動作(拖曳中)
    {
        if (!isDragging) return;

        Vector2 touchScreenPos = touchPosition.ReadValue<Vector2>();

        Vector2 currentTouchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ticketTransform.parent as RectTransform,
            touchScreenPos,
            Camera.main,
            out currentTouchPos
        );

        float deltaY = currentTouchPos.y - startTouchPos.y;
        Vector2 newPos = ticketTransform.anchoredPosition;
        newPos.y += deltaY;
        ticketTransform.anchoredPosition = newPos;

        if (ticketTransform.anchoredPosition.y >= startTouchPos.y + dragThreshold)
        {
            isDragging = false;
            OnDragComplete?.Invoke(); // 通知 GachaSystem 開始抽獎
        }
    }

    private void EndDrag() //拖曳結束(放開)
    {
        isDragging = false;
    }
}
