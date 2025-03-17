using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public bool Blocked;
    [HideInInspector] public bool Filled;

    // 用來替換的 sprites
    [SerializeField] private Sprite _blockedSprite;
    [SerializeField] private Sprite _emptySprite;
    [SerializeField] private Sprite _filledSprite;
    [SerializeField] private SpriteRenderer _cellRenderer;

    [SerializeField] private Sprite _startSprite; // 新增自定義開始點 sprite
    [SerializeField] private Sprite _endSprite; // 新增自定義結束點 sprite

    [SerializeField] private Animator animator;


    public void Init(bool isWalkable)
    {
        Blocked = !isWalkable;
        Filled = Blocked; // 如果格子是障礙物，則應該設定為已填滿

        // 用 sprite 來區分可走與不可走的格子
        _cellRenderer.sprite = Blocked ? _blockedSprite : _emptySprite;

        if (!Blocked)
        {
            animator.Play("CellIn");
        }
        else
        {
            animator.Play("Cell_Alpha0");
        }
    }

    public void Add()
    {
        Filled = true;
        _cellRenderer.sprite = _filledSprite;
    }

    public void Remove()
    {
        Filled = false;
        _cellRenderer.sprite = _emptySprite;
    }

    public void ChangeState()
    {
        Blocked = !Blocked;
        Filled = Blocked;
        _cellRenderer.sprite = Blocked ? _blockedSprite : _emptySprite;
    }

    public void SetStartSprite()
    {
        _cellRenderer.sprite = _startSprite; // 或者你可以在 Inspector 內設定自訂 sprite
    }

    public void SetEndSprite()
    {
        _cellRenderer.sprite = _endSprite; // 或者你可以在 Inspector 內設定自訂 sprite
    }

    // 如果需要替換 blocked sprite 的透明度，可以調整 sprite 的 alpha
    public void SetBlockedSpriteAlpha(float alpha)
    {
        Color currentColor = _blockedSprite.texture.GetPixel(0, 0); // 假設我們處理的是 texture 顏色
        currentColor.a = alpha;
        _cellRenderer.sprite = _blockedSprite; // 更新 sprite
        _cellRenderer.color = currentColor; // 更新透明度
    }
}