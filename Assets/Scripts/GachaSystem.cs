using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance;

    [Header("抽獎 UI 元件")]
    [SerializeField] private GameObject gachaPanel;  // 抽獎視窗
    [SerializeField] private Image ticketImage;      // 紙條圖片
    [SerializeField] private Image rewardImage;      // 抽到的獎品圖片
    [SerializeField] private Text rewardText;        // 抽到的獎品文字
    [SerializeField] private Button closeButton;     // 關閉按鈕

    [Header("拖曳控制")]
    [SerializeField] private RectTransform ticketTransform; // 紙條 RectTransform
    [SerializeField] private float dragThreshold = 150f; // 多少距離視為成功拖曳

    [Header("獎品圖案")]
    [SerializeField] private Sprite iconA; // A 圖案
    [SerializeField] private Sprite iconB; // B 圖案

    [Header("最終獎勵")]
    [SerializeField] private Sprite rewardDango;   // 糰子
    [SerializeField] private Sprite rewardBalloon; // 小氣球
    [SerializeField] private Sprite rewardStamp;   // 小郵票

    private string firstDrawResult = "";
    private string secondDrawResult = "";

    private bool isDragging = false;
    private Vector2 startPos;

    public event Action<string> OnRewardSelected; // 當獎勵選定時觸發

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        closeButton.onClick.AddListener(CloseGacha);
        gachaPanel.SetActive(false);
    }

    /// <summary>
    /// 觸發抽獎機制
    /// </summary>
    public void StartGacha()
    {
        gachaPanel.SetActive(true);
        ticketTransform.anchoredPosition = startPos; // 重置紙條位置
        rewardImage.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 處理紙條拖曳
    /// </summary>
    public void OnBeginDrag()
    {
        isDragging = true;
        startPos = ticketTransform.anchoredPosition;
    }

    public void OnDrag(Vector2 dragPosition)
    {
        if (!isDragging) return;

        ticketTransform.anchoredPosition += new Vector2(0, dragPosition.y);

        if (ticketTransform.anchoredPosition.y >= dragThreshold)
        {
            isDragging = false;
            StartCoroutine(ShowReward());
        }
    }

    private IEnumerator ShowReward()
    {
        yield return new WaitForSeconds(0.5f);

        // 設定抽到的獎品 (A 或 B)
        string drawResult = Draw();
        rewardImage.sprite = (drawResult == "A") ? iconA : iconB;
        rewardText.text = "你抽到了 " + (drawResult == "A" ? "圖案 A" : "圖案 B");

        rewardImage.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);

        // 記錄結果
        if (string.IsNullOrEmpty(firstDrawResult))
        {
            firstDrawResult = drawResult;
        }
        else
        {
            secondDrawResult = drawResult;
            DetermineFinalReward();
        }
    }

    /// <summary>
    /// 隨機抽 A 或 B
    /// </summary>
    private string Draw()
    {
        float rand = UnityEngine.Random.value;
        return (rand <= 0.5f) ? "A" : "B"; // 50% 機率
    }

    /// <summary>
    /// 決定最終獎勵
    /// </summary>
    private void DetermineFinalReward()
    {
        string finalReward = "";
        Sprite rewardSprite = null;

        if (firstDrawResult == secondDrawResult)
        {
            if (firstDrawResult == "A")
            {
                finalReward = "糰子";
                rewardSprite = rewardDango;
            }
            else
            {
                finalReward = "小氣球";
                rewardSprite = rewardBalloon;
            }
        }
        else
        {
            finalReward = "小郵票";
            rewardSprite = rewardStamp;
        }

        SaveToCloud(finalReward);
        OnRewardSelected?.Invoke(finalReward);
    }

    /// <summary>
    /// 存儲抽獎結果到雲端
    /// </summary>
    private void SaveToCloud(string reward)
    {
        PlayerPrefs.SetString("FinalReward", reward);
        PlayerPrefs.SetString("Timestamp", DateTime.Now.ToString());
        PlayerPrefs.Save();
        Debug.Log("已儲存獎勵: " + reward);
    }

    /// <summary>
    /// 關閉抽獎視窗
    /// </summary>
    private void CloseGacha()
    {
        gachaPanel.SetActive(false);
    }
}
