using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Level _level;
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Transform _edgePrefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private InputAction press,screenPos;

    private bool hasGameStart;
    private bool hasGameFinished;
    private Cell[,] cells;
    private List<Vector2Int> filledPoints;
    private List<Transform> edges;
    private Vector2Int startPos, endPos;

    private Vector3 curScreenPos;
    private bool isPressing = false; // 用來追蹤按壓狀態

    private List<Vector2Int> directions = new List<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.down,Vector2Int.left,Vector2Int.right
    };

    private void Awake()
    {
        Instance = this;
        hasGameStart = false;

    }

    private void OnEnable()
    {
        press.Enable();
        screenPos.Enable();

        // 按下滑鼠/觸控開始
        press.started += OnTouchStarted;
        // 持續拖曳時
        press.performed += OnTouchPerformed;
        // 放開滑鼠/觸控結束
        press.canceled += OnTouchCanceled;
    }

    private void OnDisable()
    {
        press.started -= OnTouchStarted;
        press.performed -= OnTouchPerformed;
        press.canceled -= OnTouchCanceled;
        press.Disable();
        screenPos.Disable();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        isPressing = true;
        curScreenPos = screenPos.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(curScreenPos);
        startPos = new Vector2Int(Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.x));
        endPos = startPos;

        // ✅ 檢查是否從 `StartPosition` 開始，否則不允許開始畫線
        if (startPos != _level.StartPosition)
        {
            isPressing = false;
            return;
        }

        // ✅ 立即標記開始格子為「填滿」
        cells[startPos.x, startPos.y].Add();

        // ✅ 初始化 `filledPoints`
        filledPoints.Clear();
        filledPoints.Add(startPos);
    }

    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        curScreenPos = screenPos.ReadValue<Vector2>();
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        isPressing = false;
        if (hasGameStart && !hasGameFinished)
        {
            StartCoroutine(ClearData());
        }
    }

    public void StartGame()
    {
        Initizlize();
    }

    private void Initizlize()
    {
        hasGameFinished = false;
        hasGameStart = true;
        filledPoints = new List<Vector2Int>();
        cells = new Cell[_level.Row, _level.Col];
        edges = new List<Transform>();
        SpawnLevel();
    }

    private IEnumerator ClearData()
    {
        hasGameFinished = false;
        filledPoints.Clear();
        foreach(var item in edges)
        {
            Destroy(item.gameObject);
        }
        edges.Clear();
        cells = new Cell[_level.Row, _level.Col];
        foreach(Transform item in _parent)
        {
            Destroy(item.gameObject);
        }
        yield return null;

        SpawnLevel();
    }

    /// <summary>
    /// 產出陣列關卡結構
    /// </summary>
    private void SpawnLevel()
    {
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = _level.Col * 0.5f;
        camPos.y = _level.Row * 0.5f;
        Camera.main.transform.position = camPos;
        Camera.main.orthographicSize = Mathf.Max(_level.Row, _level.Col) + 2f;

        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                if (cells[i, j] == null)
                {
                    cells[i, j] = Instantiate(_cellPrefab, _parent);
                    cells[i, j].transform.position = new Vector3(j + 0.5f, i + 0.5f, 0);
                }

                // ✅ **正確載入 Level 的格子狀態**
                cells[i, j].Init(_level.GetCell(i, j));

                if (new Vector2Int(i, j) == _level.StartPosition)
                    cells[i, j].SetStartColor();

                if (new Vector2Int(i, j) == _level.EndPosition)
                    cells[i, j].SetEndColor();
            }
        }
    }



    private void Update()
    {
        if (hasGameFinished || !hasGameStart || !isPressing) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos.ReadValue<Vector2>());
        endPos = new Vector2Int(Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.x));

        if (!IsNeighbour()) return;

        // ✅ 如果 `filledPoints` 只有 `startPos`，則需要建立第一條線
        if (filledPoints.Count == 1)
        {
            filledPoints.Add(endPos);
            cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddEmpty())
        {
            filledPoints.Add(startPos);
            filledPoints.Add(endPos);
            cells[startPos.x, startPos.y].Add();
            cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddToEnd())
        {
            filledPoints.Add(endPos);
            cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddToStart())
        {
            filledPoints.Insert(0, endPos);
            cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos, true);
        }
        else if (RemoveFromEnd())
        {
            RemoveLastEdge();
        }
        else if (RemoveFromStart())
        {
            RemoveFirstEdge();
        }

        RemoveEmpty();
        CheckWin();
        startPos = endPos;
    }

    private bool AddEmpty()
    {
        if (edges.Count > 0) return false;
        if (cells[startPos.x, startPos.y].Filled) return false;
        if (cells[endPos.x, endPos.y].Filled) return false;
        return true;
    }

    private bool AddToEnd()
    {
        if (filledPoints.Count < 2) return false;
        Vector2Int pos = filledPoints[filledPoints.Count - 1];
        Cell lastCell = cells[pos.x, pos.y];
        if (cells[startPos.x, startPos.y] != lastCell) return false;
        if (cells[endPos.x, endPos.y].Filled) return false;
        return true;
    }

    private bool AddToStart()
    {
        if (filledPoints.Count < 2) return false;
        Vector2Int pos = filledPoints[0];
        Cell lastCell = cells[pos.x, pos.y];
        if (cells[startPos.x, startPos.y] != lastCell) return false;
        if (cells[endPos.x, endPos.y].Filled) return false;
        return true;
    }

    private bool RemoveFromEnd()
    {
        if (filledPoints.Count < 2) return false;
        Vector2Int pos = filledPoints[filledPoints.Count - 1];
        Cell lastCell = cells[pos.x, pos.y];
        if (cells[startPos.x, startPos.y] != lastCell) return false;
        pos = filledPoints[filledPoints.Count - 2];
        lastCell = cells[pos.x, pos.y];
        if (cells[endPos.x, endPos.y] != lastCell) return false;
        return true;
    }
    private bool RemoveFromStart()
    {
        if (filledPoints.Count < 2) return false;
        Vector2Int pos = filledPoints[0];
        Cell lastCell = cells[pos.x, pos.y];
        if (cells[startPos.x, startPos.y] != lastCell) return false;
        pos = filledPoints[1];
        lastCell = cells[pos.x, pos.y];
        if (cells[endPos.x, endPos.y] != lastCell) return false;
        return true;
    }

    private void RemoveEmpty()
    {
        if (filledPoints.Count != 1) return;
        cells[filledPoints[0].x, filledPoints[0].y].Remove();
        filledPoints.RemoveAt(0);
    }

    private void SpawnEdge(Vector2Int start, Vector2Int end, bool insertAtStart = false)
    {
        Transform edge = Instantiate(_edgePrefab, _parent);
        if (insertAtStart) edges.Insert(0, edge);
        else edges.Add(edge);
        edge.transform.position = new Vector3(
            start.y * 0.5f + 0.5f + end.y * 0.5f,
            start.x * 0.5f + 0.5f + end.x * 0.5f,
            0f
        );
        bool horizontal = (end.y - start.y) != 0;
        edge.transform.eulerAngles = new Vector3(0, 0, horizontal ? 90f : 0);
    }

    private void RemoveLastEdge()
    {
        Transform removeEdge = edges[edges.Count - 1];
        edges.RemoveAt(edges.Count - 1);
        Destroy(removeEdge.gameObject);
        filledPoints.RemoveAt(filledPoints.Count - 1);
        cells[startPos.x, startPos.y].Remove();
    }

    private void RemoveFirstEdge()
    {
        Transform removeEdge = edges[0];
        edges.RemoveAt(0);
        Destroy(removeEdge.gameObject);
        filledPoints.RemoveAt(0);
        cells[startPos.x, startPos.y].Remove();
    }

    private bool IsNeighbour()
    {
        return IsValid(startPos) && IsValid(endPos) && directions.Contains(startPos - endPos);
    }

    private bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _level.Row && pos.y < _level.Col;
    }

    private void CheckWin()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                if (!cells[i, j].Filled)
                    return;
            }
        }
        // ✅ 必須讓玩家最後停在 `EndPosition` 才算勝利
        if (endPos != _level.EndPosition)
        {
            return; // 不是終點，遊戲還沒成功
        }
        hasGameFinished = true;
        StartCoroutine(GameFinished());
    }

    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Game Clear! Maybe have some score?");
        //UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
