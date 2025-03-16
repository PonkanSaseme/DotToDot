using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    public int Row;
    public int Col;
    [HideInInspector, Tooltip("使用一維陣列來存格子資訊，索引計算方式：row * Col + col")]
    public List<bool> GridData = new List<bool>();

    [Tooltip("範圍:0~Row-1")]
    public Vector2Int StartPosition; // 開始格子
    [Tooltip("範圍:0~Col-1")]
    public Vector2Int EndPosition;   // 結束格子
    [Header("自訂生成位置")]
    public Vector2 Position; //允許在 Inspector 內設定 Level 的生成座標


    private void OnValidate()
    {
        int totalCells = Row * Col;

        while (GridData.Count < totalCells)
            GridData.Add(true);
        while (GridData.Count > totalCells)
            GridData.RemoveAt(GridData.Count - 1);
    }

    public bool GetCell(int row, int col)
    {
        return GridData[row * Col + col];
    }

    public void SetCell(int row, int col, bool value)
    {
        GridData[row * Col + col] = value;
    }
}