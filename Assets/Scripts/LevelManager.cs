using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
public class LevelManager
{
    public event Action OnLevelComplete;
    //public event Action<SpriteRenderer[], float> OnFadeIn;

    private Level _level;
    private Cell[,] _cells;
    private List<Vector2Int> _filledPoints;
    private List<Transform> _edges;
    private Transform _levelParent;
    private Transform _edgePrefab;

    public LevelManager(Level level, Cell cellPrefab, Transform edgePrefab, Transform parentContainer)
    {
        _level = level;
        _edgePrefab = edgePrefab;
        _filledPoints = new List<Vector2Int>();
        _edges = new List<Transform>();

        // 初始化父物件
        _levelParent = new GameObject("Level").transform;
        _levelParent.SetParent(parentContainer);
        _levelParent.position = new Vector3(level.Position.x, level.Position.y, 0);

        // 初始化格子
        _cells = new Cell[level.Row, level.Col];
        for (int i = 0; i < level.Row; i++)
        {
            for (int j = 0; j < level.Col; j++)
            {
                bool isWalkable = level.GetCell(i, j);
                _cells[i, j] = UnityEngine.Object.Instantiate(cellPrefab, _levelParent);
                _cells[i, j].transform.position = new Vector3(j + 0.5f, i + 0.5f + level.Position.y, 0);
                _cells[i, j].Init(isWalkable);

                if (new Vector2Int(i, j) == level.StartPosition)
                {
                    _cells[i, j].SetStartColor();
                }

                if (new Vector2Int(i, j) == level.EndPosition)
                {
                    _cells[i, j].SetEndColor();
                }
            }
        }
    }

    public void CleanUp()
    {
        if (_levelParent != null)
        {
            UnityEngine.Object.Destroy(_levelParent.gameObject);
        }
    }

    public void HandleTouchStart(Vector2Int startPos)
    {
        if (!IsValid(startPos)) return;
        if (startPos != _level.StartPosition) return;

        if (_cells[startPos.x, startPos.y] is Cell cell)
        {
            cell.Add();
        }
        else
        {
            return;
        }

        _filledPoints.Clear();
        _filledPoints.Add(startPos);
    }

    public void HandleTouchMove(Vector2Int endPos)
    {
        if (!IsValid(endPos) || _filledPoints.Count == 0)
        {
            return;
        }

        Vector2Int startPos = _filledPoints[_filledPoints.Count - 1];

        if (!IsNeighbour(startPos, endPos))
        {
            return;
        }

        if (!_cells[endPos.x, endPos.y].Filled)
        {
            _filledPoints.Add(endPos);
            _cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }

        CheckWin();
    }

    private bool IsValid(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= _level.Row || pos.y >= _level.Col)
            return false;

        if (!_level.GetCell(pos.x, pos.y))
            return false;

        return true;
    }

    private bool IsNeighbour(Vector2Int startPos, Vector2Int endPos)
    {
        // 曼哈頓距離為 1 表示這兩個格子是相鄰的
        return Mathf.Abs(startPos.x - endPos.x) + Mathf.Abs(startPos.y - endPos.y) == 1;
    }

    private void SpawnEdge(Vector2Int start, Vector2Int end, bool insertAtStart = false)
    {
        Transform edge = UnityEngine.Object.Instantiate(_edgePrefab, _levelParent);
        if (insertAtStart)
            _edges.Insert(0, edge);
        else
            _edges.Add(edge);

        float yOffset = _levelParent.position.y;

        // 確保生成的邊線在格子之間的正中間
        edge.transform.position = new Vector3(
            (start.y + end.y) / 2f + 0.5f,
            (start.x + end.x) / 2f + 0.5f + yOffset,
            0f
        );

        // 判斷邊線是水平還是垂直，並進行旋轉
        bool horizontal = (end.y - start.y) != 0;
        edge.transform.eulerAngles = new Vector3(0, 0, horizontal ? 90f : 0);
    }

    private void RemoveLastEdge()
    {
        if (_edges.Count == 0) return;

        Transform removeEdge = _edges[_edges.Count - 1];
        _edges.RemoveAt(_edges.Count - 1);
        UnityEngine.Object.Destroy(removeEdge.gameObject);

        if (_filledPoints.Count > 0)
        {
            _filledPoints.RemoveAt(_filledPoints.Count - 1);
        }

        if (_filledPoints.Count > 0)
        {
            Vector2Int lastPos = _filledPoints[_filledPoints.Count - 1];
            _cells[lastPos.x, lastPos.y].Remove();
        }
    }

    private void RemoveFirstEdge()
    {
        if (_edges.Count == 0) return;

        Transform removeEdge = _edges[0];
        _edges.RemoveAt(0);
        UnityEngine.Object.Destroy(removeEdge.gameObject);

        if (_filledPoints.Count > 0)
        {
            _filledPoints.RemoveAt(0);
        }

        if (_filledPoints.Count > 0)
        {
            Vector2Int firstPos = _filledPoints[0];
            _cells[firstPos.x, firstPos.y].Remove();
        }
    }

    public Vector3 GetLevelPosition()
    {
        return _levelParent.position;
    }

    public Vector3 GetStartIconPosition()
    {
        return new Vector3(
            _level.StartPosition.y + 0.5f, // X 軸 (Col)
            _level.StartPosition.x + 0.5f, // Y 軸 (Row)
            0
        );
    }

    private void CheckWin()
    {
        bool isComplete = true;

        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                if (!_cells[i, j].Filled)
                {
                    isComplete = false;
                    break;
                }
            }
        }

        if (isComplete && _filledPoints[_filledPoints.Count - 1] == _level.EndPosition)
        {
            OnLevelComplete?.Invoke();
        }
    }
}