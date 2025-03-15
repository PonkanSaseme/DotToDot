using System;
using UnityEngine;

public class Lottery : MonoBehaviour
{
    void Start()
    {
        string result = Draw();
        Debug.Log("抽獎結果: " + result);
    }

    string Draw()
    {
        // 定義獎項和對應的機率
        (string, float)[] prizes = new (string, float)[]
        {
            ("AA", 0.01f),
            ("BB", 0.07f),
            ("AB", 0.0767f), ("BA", 0.0767f),
            ("AC", 0.0767f), ("CA", 0.0767f),
            ("BC", 0.0767f), ("CB", 0.0767f),
            ("AD", 0.0767f), ("DA", 0.0767f),
            ("BD", 0.0767f), ("DB", 0.0767f),
            ("CD", 0.0767f), ("DC", 0.0767f)
        };

        float rand = UnityEngine.Random.value; // 取得 0~1 之間的隨機數
        float cumulative = 0f;

        foreach (var prize in prizes)
        {
            cumulative += prize.Item2;
            if (rand < cumulative)
            {
                return prize.Item1;
            }
        }

        return "ERROR"; // 應該不會發生，但加個保險
    }
}
