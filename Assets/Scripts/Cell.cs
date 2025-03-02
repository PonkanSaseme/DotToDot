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
        Filled = false; // ��l�Ʈɤ��i��R

        // �]�w�C��
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
        _cellRenderer.color = Color.green; // �Ϊ̧A�i�H�b Inspector ���]�w�ۭq�C��
    }

    public void SetEndColor()
    {
        _cellRenderer.color = Color.red; // �Ϊ̧A�i�H�b Inspector ���]�w�ۭq�C��
    }
}
