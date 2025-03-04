using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public bool Blocked;
    [HideInInspector] public bool Filled;

    [SerializeField] private Color _blockedColor;
    [SerializeField] private Color _emptyColor;
    [SerializeField] private Color _filledColor;
    [SerializeField] private SpriteRenderer _cellRenderer;
    [SerializeField] private Color _startColor; // 新增自定義開始點顏色
    [SerializeField] private Color _endColor; // 新增自定義結束點顏色

    public void Init(bool isWalkable)
    {
        Blocked = !isWalkable;
        Filled = Blocked; //**如果格子是障礙物，則應該設定為已填滿**

        //**用顏色來區分可走與不可走的格子**
        _cellRenderer.color = Blocked ? _blockedColor : _emptyColor;
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
        _cellRenderer.color = _startColor; // 或者你可以在 Inspector 內設定自訂顏色
    }

    public void SetEndColor()
    {
        _cellRenderer.color = _endColor; // 或者你可以在 Inspector 內設定自訂顏色
    }
}
