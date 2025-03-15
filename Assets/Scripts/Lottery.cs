using System;
using UnityEngine;

public class Lottery : MonoBehaviour
{
    void Start()
    {
        string result = Draw();
        Debug.Log("������G: " + result);
    }

    string Draw()
    {
        // �w�q�����M���������v
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

        float rand = UnityEngine.Random.value; // ���o 0~1 �������H����
        float cumulative = 0f;

        foreach (var prize in prizes)
        {
            cumulative += prize.Item2;
            if (rand < cumulative)
            {
                return prize.Item1;
            }
        }

        return "ERROR"; // ���Ӥ��|�o�͡A���[�ӫO�I
    }
}
