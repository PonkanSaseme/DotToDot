using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelManager
{
    public event Action OnLevelComplete;

    private Level _level;
    private Cell[,] _cells;
    private List<Vector2Int> _filledPoints;
    private List<Transform> _edges;
    private Transform _levelParent;
    private Transform _edgePrefab;
    private int currentLevelIndex = 0; // �l�ܷ�e�ާ@�� Level

    public LevelManager(Level level, Cell cellPrefab, Transform edgePrefab, Transform parentContainer)
    {
        _level = level;
        _edgePrefab = edgePrefab;
        _filledPoints = new List<Vector2Int>();
        _edges = new List<Transform>();

        if (level == null)
        {
            Debug.LogError("Level �� null�A�L�k��l�� LevelManager�I");
            return;
        }

        // ��l�Ƥ�����
        _levelParent = new GameObject("Level").transform;
        _levelParent.SetParent(parentContainer);
        _levelParent.position = new Vector3(level.Position.x, level.Position.y, 0);

        // ��l�Ʈ�l
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
        if (_cells == null)
        {
            Debug.LogError("_cells �}�C�|����l�ơI");
            return;
        }


        if (!IsValid(startPos,currentLevelIndex))
        {
            Debug.LogWarning($"�L�Ī��_�l��m: {startPos}");
            return;
        }

        if (startPos != _level.StartPosition)
        {
            Debug.LogWarning($"{startPos} ���O�_�l�I {_level.StartPosition}");
            return;
        }

        if (_cells[startPos.x, startPos.y] is Cell cell)
        {
            cell.Add();
        }
        else
        {
            Debug.LogError($"�L�k�񺡮�l�Acells �|����l�ơI");
            return;
        }

        _filledPoints.Clear();
        _filledPoints.Add(startPos);
    }

    public void HandleTouchMove(Vector2Int endPos)
    {
        if (_cells == null)
        {
            Debug.LogError("_cells �}�C�|����l�ơI");
            return;
        }

        if (!IsValid(endPos, currentLevelIndex) || _filledPoints.Count == 0)
        {
            Debug.LogWarning($"�L�Ī����ʥؼ�: {endPos}");
            return;
        }

        Vector2Int startPos = _filledPoints[_filledPoints.Count - 1];
        Debug.Log($"���յe�u: {startPos} -> {endPos}");

        if (!IsNeighbour(startPos, endPos))
        {
            Debug.LogWarning("�o����I���O�۾F���I");
            return;
        }
        Debug.Log("�o����I�O�۾F���A�}�l�e�u");

        // �T�O _cells[endPos] ���O null
        if (_cells[endPos.x, endPos.y] == null)
        {
            Debug.LogError($"_cells[{endPos.x}, {endPos.y}] �O null�A�L�k�e�u�I");
            return;
        }
        // **ø�s�u���üаO Cell**
        _filledPoints.Add(endPos);
        _cells[endPos.x, endPos.y].Add();  // �ܧ��C��
        SpawnEdge(startPos, endPos);
        Debug.Log($"�w���\�e�u {startPos} -> {endPos}");

        if (_filledPoints.Count == 1)
        {
            _filledPoints.Add(endPos);
            _cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddEmpty(startPos, endPos))
        {
            _filledPoints.Add(startPos);
            _filledPoints.Add(endPos);
            _cells[startPos.x, startPos.y].Add();
            _cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddToEnd(startPos, endPos))
        {
            _filledPoints.Add(endPos);
            _cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos);
        }
        else if (AddToStart(startPos, endPos))
        {
            _filledPoints.Insert(0, endPos);
            _cells[endPos.x, endPos.y].Add();
            SpawnEdge(startPos, endPos, true);
        }
        else if (RemoveFromEnd(startPos, endPos))
        {
            RemoveLastEdge();
        }
        else if (RemoveFromStart(startPos, endPos))
        {
            RemoveFirstEdge();
        }

        RemoveEmpty();
        CheckWin();
    }

    private bool IsNeighbour(Vector2Int startPos, Vector2Int endPos)
    {
        return IsValid(startPos, currentLevelIndex) &&
               IsValid(endPos, currentLevelIndex) &&
               (Mathf.Abs(startPos.x - endPos.x) + Mathf.Abs(startPos.y - endPos.y) == 1);
    }
    private bool IsValid(Vector2Int pos, int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _level.Row) return false; //�ˬdlevelIndex
        if (pos.x < 0 || pos.y < 0 || pos.x >= _level.Row || pos.y >= _level.Col)
            return false;

        if (!_level.GetCell(pos.x, pos.y)) //�ˬd�O�_����ê��
            return false;

        return true;
    }


    private bool AddEmpty(Vector2Int startPos, Vector2Int endPos)
    {
        if (!IsValid(startPos, currentLevelIndex) || !IsValid(endPos, currentLevelIndex))
            return false;

        if (_edges.Count > 0) return false;
        if (_cells[startPos.x, startPos.y].Filled) return false;
        if (_cells[endPos.x, endPos.y].Filled) return false;

        if (!_level.GetCell(startPos.x, startPos.y)) return false;
        if (!_level.GetCell(endPos.x, endPos.y)) return false;

        return true;
    }

    private bool AddToEnd(Vector2Int startPos, Vector2Int endPos)
    {
        if (_filledPoints.Count < 2) return false;
        Vector2Int pos = _filledPoints[_filledPoints.Count - 1];
        if (_cells[startPos.x, startPos.y] != _cells[pos.x, pos.y]) return false;
        if (_cells[endPos.x, endPos.y].Filled) return false;
        if (!_level.GetCell(endPos.x, endPos.y)) return false;
        return true;
    }

    private bool AddToStart(Vector2Int startPos, Vector2Int endPos)
    {
        if (_filledPoints.Count < 2) return false;

        Vector2Int pos = _filledPoints[0];
        if (_cells[startPos.x, startPos.y] != _cells[pos.x, pos.y]) return false;
        if (_cells[endPos.x, endPos.y].Filled) return false;
        if (!_level.GetCell(endPos.x, endPos.y)) return false;
        return true;
    }

    private bool RemoveFromEnd(Vector2Int startPos, Vector2Int endPos)
    {
        if (_filledPoints.Count < 2) return false;

        Vector2Int pos = _filledPoints[_filledPoints.Count - 1];
        Cell lastCell = _cells[pos.x, pos.y];

        if (_cells[startPos.x, startPos.y] != lastCell) return false;

        pos = _filledPoints[_filledPoints.Count - 2];
        lastCell = _cells[pos.x, pos.y];

        if (_cells[endPos.x, endPos.y] != lastCell) return false;

        Transform removeEdge = _edges[_edges.Count - 1];
        _edges.RemoveAt(_edges.Count - 1);
        UnityEngine.Object.Destroy(removeEdge.gameObject);

        _filledPoints.RemoveAt(_filledPoints.Count - 1);
        _cells[startPos.x, startPos.y].Remove();

        return true;
    }

    private bool RemoveFromStart(Vector2Int startPos, Vector2Int endPos)
    {
        if (_filledPoints.Count < 2) return false;

        Vector2Int pos = _filledPoints[0];
        Cell firstCell = _cells[pos.x, pos.y];

        if (_cells[startPos.x, startPos.y] != firstCell) return false;

        pos = _filledPoints[1];
        firstCell = _cells[pos.x, pos.y];

        if (_cells[endPos.x, endPos.y] != firstCell) return false;

        Transform removeEdge = _edges[0];
        _edges.RemoveAt(0);
        UnityEngine.Object.Destroy(removeEdge.gameObject);

        _filledPoints.RemoveAt(0);
        _cells[startPos.x, startPos.y].Remove();

        return true;
    }

    private void RemoveEmpty()
    {
        if (_filledPoints.Count != 1) return;
        _cells[_filledPoints[0].x, _filledPoints[0].y].Remove();
        _filledPoints.RemoveAt(0);
    }

    private void SpawnEdge(Vector2Int start, Vector2Int end, bool insertAtStart = false)
    {
        Debug.Log($"���եͦ���u: {start} -> {end}");

        Transform edge = UnityEngine.Object.Instantiate(_edgePrefab, _levelParent);
        if (insertAtStart)
            _edges.Insert(0, edge);
        else
            _edges.Add(edge);

        float yOffset = _levelParent.position.y;

        edge.transform.position = new Vector3(
            start.y * 0.5f + 0.5f + end.y * 0.5f,
            start.x * 0.5f + 0.5f + end.x * 0.5f + yOffset,
            0f
        );

        bool horizontal = (end.y - start.y) != 0;
        edge.transform.eulerAngles = new Vector3(0, 0, horizontal ? 90f : 0);

        Debug.Log($"�w���\�ͦ���u {start} -> {end}");
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