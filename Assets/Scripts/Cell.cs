using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public bool Blocked;
    [HideInInspector] public bool Filled;

    [SerializeField] private Color _blockedColor;
    [SerializeField] private Color _emptyColor;
    [SerializeField] private Color _filledColor;
    [SerializeField] private SpriteRenderer _cellRenderer;

    public void Init(bool isBlocked)
    {
        Blocked = isBlocked;
        Filled = false; // 初始化時不可填充

        // 設定顏色
        _cellRenderer.color = Blocked ? _emptyColor : _blockedColor;
    }

    public void Add()
    {
        Filled = true;
        _cellRenderer.color = _filledColor;
    }

    public void Remove()
    {
        Filled = false;
        _cellRenderer.color = _emptyColor;
    }

    public void ChangeState()
    {
        Blocked = !Blocked;
        Filled = Blocked;
        _cellRenderer.color = Blocked ? _emptyColor : _blockedColor;
    }

    public void SetStartColor()
    {
        _cellRenderer.color = Color.green; // 或者你可以在 Inspector 內設定自訂顏色
    }

    public void SetEndColor()
    {
        _cellRenderer.color = Color.red; // 或者你可以在 Inspector 內設定自訂顏色
    }
}
