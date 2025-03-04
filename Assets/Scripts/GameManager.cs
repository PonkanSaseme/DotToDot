using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private List<Level> _levels; // 改成 List，支援多個 Level
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Transform _edgePrefab;
    [SerializeField] private Transform _parentContainer; // 最上層的父物件，管理所有關卡
    [SerializeField] private InputAction press, screenPos;

    private bool hasGameStart;
    private bool hasGameFinished;

    private List<Cell[,]> cellsList = new List<Cell[,]>(); // 用 List 存多個關卡的 cells
    private List<List<Vector2Int>> filledPointsList = new List<List<Vector2Int>>(); // 每個 Level 有自己的 filledPoints
    private List<List<Transform>> edgesList = new List<List<Transform>>(); // 每個 Level 有自己的 edges
    private List<Transform> levelParents = new List<Transform>(); // 每個 Level 有獨立的 parent

    private int currentLevelIndex = 0; // 追蹤當前操作的 Level
    private Vector2Int startPos, endPos;
    private Vector3 curScreenPos;
    private bool isPressing = false; // 用來追蹤按壓狀態

    private List<Vector2Int> directions = new List<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private void Awake()
    {
        Instance = this;
        hasGameStart = false;
    }

    private void Start()
    {
        StartGame();
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
        if (!_parentContainer.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("❌ 父物件尚未開啟，取消觸控偵測！");
            return;
        }

        isPressing = true;
        curScreenPos = screenPos.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(curScreenPos);

        startPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
        endPos = startPos;

        //確保startPos在cellsList的範圍內
        if (!IsValid(startPos, currentLevelIndex))
        {
            isPressing = false;
            return;
        }

        //確保startPos是該Level的StartPosition
        if (startPos != _levels[currentLevelIndex].StartPosition)
        {
            isPressing = false;
            return;
        }

        //確保Cell存在並能夠填滿
        if (cellsList[currentLevelIndex] != null && cellsList[currentLevelIndex][startPos.x, startPos.y] != null)
        {
            cellsList[currentLevelIndex][startPos.x, startPos.y].Add();
        }
        else
        {
            Debug.LogError($"❌ 無法填滿格子，cellsList[{currentLevelIndex}] 尚未初始化！");
        }

        filledPointsList[currentLevelIndex].Clear();
        filledPointsList[currentLevelIndex].Add(startPos);
    }


    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        curScreenPos = screenPos.ReadValue<Vector2>();

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(curScreenPos);
        endPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y)); //確保endPos會更新
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
        //確保 parent 父物件開啟
        _parentContainer.gameObject.SetActive(true);

        Initialize();
    }


    private void Initialize()
    {
        hasGameFinished = false;
        hasGameStart = true;

        cellsList.Clear();
        filledPointsList.Clear();
        edgesList.Clear();
        levelParents.Clear();

        // 清除舊的關卡 Parent（避免重複生成）
        foreach (Transform child in _parentContainer)
        {
            Destroy(child.gameObject);
        }

        SpawnLevels();
    }

    private IEnumerator ClearData()
    {
        hasGameFinished = false;

        foreach (Transform levelParent in levelParents)
        {
            Destroy(levelParent.gameObject);
        }
        levelParents.Clear();

        cellsList.Clear();
        filledPointsList.Clear();
        edgesList.Clear();

        yield return null;
        SpawnLevels();
    }

    private void SpawnLevels()
    {
        float yOffset = 0f; //控制 Y 軸偏移量，讓 Level 按照高度排列

        for (int levelIndex = 0; levelIndex < _levels.Count; levelIndex++)
        {
            Level level = _levels[levelIndex];

            //讓每個 Level 依照前一個 Level 的高度排列
            if (levelIndex > 0)
            {
                yOffset += _levels[levelIndex - 1].Row + 2; //+2是額外間距，避免貼太緊
            }

            //為每個關卡創建獨立的 Parent
            Transform levelParent = new GameObject($"Level_{levelIndex + 1}").transform;
            levelParent.SetParent(_parentContainer);
            levelParent.position = new Vector3(0, yOffset, 0); //Y 軸根據yOffset設定
            levelParents.Add(levelParent);

            Cell[,] cells = new Cell[level.Row, level.Col];
            cellsList.Add(cells);
            filledPointsList.Add(new List<Vector2Int>());
            edgesList.Add(new List<Transform>());

            for (int i = 0; i < level.Row; i++)
            {
                for (int j = 0; j < level.Col; j++)
                {
                    bool isWalkable = level.GetCell(i, j);

                    cells[i, j] = Instantiate(_cellPrefab, levelParent);
                    cells[i, j].transform.position = new Vector3(j + 0.5f, i + 0.5f + yOffset, 0); //設定正確的 Y 位置
                    cells[i, j].Init(isWalkable);

                    if (new Vector2Int(i, j) == level.StartPosition)
                        cells[i, j].SetStartColor();

                    if (new Vector2Int(i, j) == level.EndPosition)
                        cells[i, j].SetEndColor();
                }
            }
        }

        AdjustCamera(); //自動調整攝影機，確保所有 Level 可見
    }
    private void Update()
    {
        if (hasGameFinished || !hasGameStart || !isPressing) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos.ReadValue<Vector2>());
        endPos = new Vector2Int(Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.x));

        if (!IsValid(startPos, currentLevelIndex) || !IsValid(endPos, currentLevelIndex))
            return; //先檢查範圍

        if (!IsNeighbour()) return;

        //如果 filledPoints 只有 startPos，則需要建立第一條線
        if (filledPointsList[currentLevelIndex].Count == 1)
        {
            filledPointsList[currentLevelIndex].Add(endPos);
            cellsList[currentLevelIndex][endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddEmpty(currentLevelIndex))
        {
            filledPointsList[currentLevelIndex].Add(startPos);
            filledPointsList[currentLevelIndex].Add(endPos);
            cellsList[currentLevelIndex][startPos.x, startPos.y].Add();
            cellsList[currentLevelIndex][endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddToEnd(currentLevelIndex))
        {
            filledPointsList[currentLevelIndex].Add(endPos);
            cellsList[currentLevelIndex][endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddToStart(currentLevelIndex))
        {
            filledPointsList[currentLevelIndex].Insert(0, endPos);
            cellsList[currentLevelIndex][endPos.x, endPos.y].Add();
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

        //**每次畫線後，檢查是否完成關卡**
        CheckWin();

        startPos = endPos;
    }

    private bool AddEmpty(int levelIndex)
    {
        //**先確保 `startPos` 和 `endPos` 在合法範圍內**
        if (!IsValid(startPos, levelIndex) || !IsValid(endPos, levelIndex))
            return false;

        if (edgesList[levelIndex].Count > 0) return false;
        if (cellsList[levelIndex][startPos.x, startPos.y].Filled) return false;
        if (cellsList[levelIndex][endPos.x, endPos.y].Filled) return false;

        //**檢查 GridData，確保這條路線不穿過障礙物**
        if (!_levels[levelIndex].GetCell(startPos.x, startPos.y)) return false;
        if (!_levels[levelIndex].GetCell(endPos.x, endPos.y)) return false;

        return true;
    }



    private bool AddToEnd(int levelIndex)
    {
        if (filledPointsList[levelIndex].Count < 2) return false;
        Vector2Int pos = filledPointsList[levelIndex][filledPointsList[levelIndex].Count - 1];
        if (cellsList[levelIndex][startPos.x, startPos.y] != cellsList[levelIndex][pos.x, pos.y]) return false;
        if (cellsList[levelIndex][endPos.x, endPos.y].Filled) return false;
        if (!_levels[levelIndex].GetCell(endPos.x, endPos.y)) return false;
        return true;
    }


    private bool AddToStart(int levelIndex)
    {
        if (filledPointsList[levelIndex].Count < 2) return false;

        Vector2Int pos = filledPointsList[levelIndex][0];
        if (cellsList[levelIndex][startPos.x, startPos.y] != cellsList[levelIndex][pos.x, pos.y]) return false;
        if (cellsList[levelIndex][endPos.x, endPos.y].Filled) return false;

        //**確保不能畫線到障礙物**
        if (!_levels[levelIndex].GetCell(endPos.x, endPos.y)) return false;

        return true;
    }


    private bool RemoveFromEnd()
    {
        if (filledPointsList[currentLevelIndex].Count < 2) return false;

        Vector2Int pos = filledPointsList[currentLevelIndex][filledPointsList[currentLevelIndex].Count - 1];
        Cell lastCell = cellsList[currentLevelIndex][pos.x, pos.y];

        if (cellsList[currentLevelIndex][startPos.x, startPos.y] != lastCell) return false;

        pos = filledPointsList[currentLevelIndex][filledPointsList[currentLevelIndex].Count - 2];
        lastCell = cellsList[currentLevelIndex][pos.x, pos.y];

        if (cellsList[currentLevelIndex][endPos.x, endPos.y] != lastCell) return false;

        //移除最後一條線
        Transform removeEdge = edgesList[currentLevelIndex][edgesList[currentLevelIndex].Count - 1];
        edgesList[currentLevelIndex].RemoveAt(edgesList[currentLevelIndex].Count - 1);
        Destroy(removeEdge.gameObject);

        //移除最後一個點
        filledPointsList[currentLevelIndex].RemoveAt(filledPointsList[currentLevelIndex].Count - 1);
        cellsList[currentLevelIndex][startPos.x, startPos.y].Remove();

        return true;
    }

    private bool RemoveFromStart()
    {
        if (filledPointsList[currentLevelIndex].Count < 2) return false;

        Vector2Int pos = filledPointsList[currentLevelIndex][0];
        Cell firstCell = cellsList[currentLevelIndex][pos.x, pos.y];

        if (cellsList[currentLevelIndex][startPos.x, startPos.y] != firstCell) return false;

        pos = filledPointsList[currentLevelIndex][1];
        firstCell = cellsList[currentLevelIndex][pos.x, pos.y];

        if (cellsList[currentLevelIndex][endPos.x, endPos.y] != firstCell) return false;

        //移除第一條線
        Transform removeEdge = edgesList[currentLevelIndex][0];
        edgesList[currentLevelIndex].RemoveAt(0);
        Destroy(removeEdge.gameObject);

        //移除最前面一個點
        filledPointsList[currentLevelIndex].RemoveAt(0);
        cellsList[currentLevelIndex][startPos.x, startPos.y].Remove();

        return true;
    }


    private void RemoveEmpty()
    {
        if (filledPointsList[currentLevelIndex].Count != 1) return;
        cellsList[currentLevelIndex][filledPointsList[currentLevelIndex][0].x, filledPointsList[currentLevelIndex][0].y].Remove();
        filledPointsList[currentLevelIndex].RemoveAt(0);
    }

    private void SpawnEdge(Vector2Int start, Vector2Int end, bool insertAtStart = false)
    {
        Transform edge = Instantiate(_edgePrefab, levelParents[currentLevelIndex]); //使用正確的 Parent
        if (insertAtStart)
            edgesList[currentLevelIndex].Insert(0, edge);
        else
            edgesList[currentLevelIndex].Add(edge);

        float yOffset = levelParents[currentLevelIndex].position.y; //加上 Level 偏移量

        edge.transform.position = new Vector3(
            start.y * 0.5f + 0.5f + end.y * 0.5f,
            start.x * 0.5f + 0.5f + end.x * 0.5f + yOffset, //修正 Y 位置
            0f
        );

        bool horizontal = (end.y - start.y) != 0;
        edge.transform.eulerAngles = new Vector3(0, 0, horizontal ? 90f : 0);
    }



    private void RemoveLastEdge()
    {
        if (edgesList[currentLevelIndex].Count == 0) return; //避免空陣列錯誤

        Transform removeEdge = edgesList[currentLevelIndex][edgesList[currentLevelIndex].Count - 1];
        edgesList[currentLevelIndex].RemoveAt(edgesList[currentLevelIndex].Count - 1);
        Destroy(removeEdge.gameObject);

        if (filledPointsList[currentLevelIndex].Count > 0)
        {
            filledPointsList[currentLevelIndex].RemoveAt(filledPointsList[currentLevelIndex].Count - 1);
        }

        if (filledPointsList[currentLevelIndex].Count > 0)
        {
            Vector2Int lastPos = filledPointsList[currentLevelIndex][filledPointsList[currentLevelIndex].Count - 1];
            cellsList[currentLevelIndex][lastPos.x, lastPos.y].Remove();
        }
    }


    private void RemoveFirstEdge()
    {
        if (edgesList[currentLevelIndex].Count == 0) return; //避免空陣列錯誤

        Transform removeEdge = edgesList[currentLevelIndex][0];
        edgesList[currentLevelIndex].RemoveAt(0);
        Destroy(removeEdge.gameObject);

        if (filledPointsList[currentLevelIndex].Count > 0)
        {
            filledPointsList[currentLevelIndex].RemoveAt(0);
        }

        if (filledPointsList[currentLevelIndex].Count > 0)
        {
            Vector2Int firstPos = filledPointsList[currentLevelIndex][0];
            cellsList[currentLevelIndex][firstPos.x, firstPos.y].Remove();
        }
    }


    private bool IsNeighbour()
    {
        return IsValid(startPos, currentLevelIndex) &&
               IsValid(endPos, currentLevelIndex) &&
               directions.Contains(endPos - startPos); //修正方向計算
    }


    private bool IsValid(Vector2Int pos, int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Count) return false; //檢查levelIndex
        if (pos.x < 0 || pos.y < 0 || pos.x >= _levels[levelIndex].Row || pos.y >= _levels[levelIndex].Col)
            return false;

        if (!_levels[levelIndex].GetCell(pos.x, pos.y)) //檢查是否為障礙物
            return false;

        return true;
    }


    private void CheckWin()
    {
        bool isComplete = true;

        for (int i = 0; i < _levels[currentLevelIndex].Row; i++)
        {
            for (int j = 0; j < _levels[currentLevelIndex].Col; j++)
            {
                if (!cellsList[currentLevelIndex][i, j].Filled)
                {
                    isComplete = false;
                    break;
                }
            }
        }

        //如果關卡完成，且玩家最後停在EndPosition，則遊戲勝利
        if (isComplete && endPos == _levels[currentLevelIndex].EndPosition)
        {
            hasGameFinished = true;
            StartCoroutine(GameFinished());
        }
    }

    private void AdjustCamera()
    {
        if (_levels.Count == 0) return;

        float minY = 0f;
        float maxY = 0f;

        for (int i = 0; i < _levels.Count; i++)
        {
            maxY += _levels[i].Row + 2; //計算所有 Level 的總高度
        }

        //設定攝影機中心點
        Vector3 camPos = new Vector3(0, maxY / 2, -10f);
        Camera.main.transform.position = camPos;

        //調整攝影機大小，確保所有 Level 可見
        Camera.main.orthographicSize = maxY / 2 + 2;
    }
    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Game Clear! Maybe have some score?");

        //這裡可以加上勝利畫面或轉場動畫
        //Example: 顯示一個 UI 來通知玩家通關成功
        //UIManager.Instance.ShowWinScreen();

        //如果你想讓遊戲自動重啟，可以解開這行
        // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}

