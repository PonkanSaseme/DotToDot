using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    private Level _level;

    private void OnEnable()
    {
        _level = (Level)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 繪製 `Level` 其他預設 Inspector 內容

        GUILayout.Space(10);
        GUILayout.Label("🔳 編輯關卡 GridData", EditorStyles.boldLabel);

        if (_level.GridData == null || _level.GridData.Count != _level.Row * _level.Col)
        {
            GUILayout.Label("⚠️ GridData 尚未初始化或大小錯誤", EditorStyles.boldLabel);
            return;
        }

        // **繪製二維表格來顯示格子狀態**
        for (int i = _level.Row-1; i >=0; i--)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < _level.Col; j++)
            {
                int index = i * _level.Col + j; // 計算索引

                // **顯示 [row, col] 標籤**
                GUILayout.Label($"[{i},{j}]", GUILayout.Width(40));

                // **用 Toggle 來編輯格子狀態**
                _level.GridData[index] = EditorGUILayout.Toggle(_level.GridData[index]);
            }
            GUILayout.EndHorizontal();
        }

        // **如果有變更，確保 Unity 存檔**
        if (GUI.changed)
        {
            EditorUtility.SetDirty(_level);
            AssetDatabase.SaveAssets();
        }
    }
}

