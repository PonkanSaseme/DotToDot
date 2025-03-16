using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    public int Row;
    public int Col;
    [HideInInspector, Tooltip("�ϥΤ@���}�C�Ӧs��l��T�A���ޭp��覡�Grow * Col + col")]
    public List<bool> GridData = new List<bool>();

    [Tooltip("�d��:0~Row-1")]
    public Vector2Int StartPosition; // �}�l��l
    [Tooltip("�d��:0~Col-1")]
    public Vector2Int EndPosition;   // ������l
    [Header("�ۭq�ͦ���m")]
    public Vector2 Position; //���\�b Inspector ���]�w Level ���ͦ��y��


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