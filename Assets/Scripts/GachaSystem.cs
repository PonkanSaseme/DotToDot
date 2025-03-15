using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance;

    [Header("抽獎 UI 元件")]
    public GameObject gachaPanel;  // 抽獎視窗
    [SerializeField] private Image ticketImage;      // 紙條圖片
    [SerializeField] private Image rewardImage;      // 抽到的獎品圖片
    [SerializeField] private Image finalRewardImage; // 最終獎品圖片
    [SerializeField] private Text finalRewardText; // 最終獎品文字
    string finalReward = "";
    [SerializeField] private Text rewardText;        // 抽到的獎品文字
    [SerializeField] private Button closeButton;     // 關閉按鈕
    public GameObject resultScene; // 抽獎結果畫面

    [Header("動畫元件")]
    [SerializeField] private GameObject paperAnim;  // 抽獎動畫

    [Header("獎品圖案")]
    [SerializeField] private Sprite iconA; // A 圖案
    [SerializeField] private Sprite iconB; // B 圖案
    [SerializeField] private Sprite iconC; // C 圖案 (銘謝惠顧)
    [SerializeField] private Sprite iconD; // D 圖案 (銘謝惠顧)

    [Header("最終獎勵")]
    [SerializeField] private Sprite rewardDango;   // 糰子
    [SerializeField] private Sprite rewardBalloon; // 小氣球
    [SerializeField] private Sprite rewardStamp;   // 小郵票

    List<Sprite> rewardSprites = new List<Sprite>();

    public event Action<string> OnRewardSelected; // 當獎勵選定時觸發
    Animator parentAnimator;
    Animator resultAnim;

    public event Action OnNextLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        gachaPanel.SetActive(false);
    }

    /// <summary>
    /// 觸發抽獎機制
    /// </summary>
    public void StartGacha() //開始抽獎流程
    {
        parentAnimator = gachaPanel.GetComponent<Animator>();
        parentAnimator.Play("PaperDragAnim");
        StartCoroutine(CheckAniEnd());
    }

    private IEnumerator CheckAniEnd()
    {
        while (!parentAnimator.GetCurrentAnimatorStateInfo(0).IsName("GachaFadeOutAnim"))
        {
            yield return null;
        }
        while (parentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        StartCoroutine(ShowReward());
    }

    private IEnumerator ShowReward()
    {
        yield return new WaitForSeconds(0.5f);

        // 設定抽到的最終獎品
        string drawResult = Draw();

        yield return null;

        DetermineFinalReward(drawResult); //最終結果獎品

        // 等待動畫完成

        // 關閉抽獎畫面，開啟結果畫面
        gachaPanel.SetActive(false);
        resultScene.SetActive(true);
        Debug.Log($"開啟結果 {drawResult}");

        // 播放結果動畫（如果有）
        resultAnim = resultScene.GetComponent<Animator>();

        // 更新最終結果圖片
        finalRewardImage.sprite = rewardSprites[rewardSprites.Count - 1];
        finalRewardText.text = finalReward;
    }

    public void CloseResult()
    {
        resultAnim.Play("ResultFadeOutAnim");
        OnNextLevel?.Invoke();
    }

    //抽獎機率計算
    public string Draw()
    {
        // 定義獎項和對應的機率
        (string, float)[] prizes = new (string, float)[] //0~1之間去分配機率
        {
            ("AA", 0.01f),
            ("BB", 0.07f),
            ("AB", 0.0657f), ("BA", 0.0657f),
            ("AC", 0.0657f), ("CA", 0.0657f),
            ("BC", 0.0657f), ("CB", 0.0657f),
            ("AD", 0.0657f), ("DA", 0.0657f),
            ("BD", 0.0657f), ("DB", 0.0657f),
            ("CD", 0.0657f), ("DC", 0.0657f),
            ("CC", 0.0657f), ("DD", 0.0657f)
        };

        float rand = UnityEngine.Random.value; // 取得 0~1 之間的隨機數
        float cumulative = 0f; //累積數字

        foreach (var prize in prizes)
        {
            cumulative += prize.Item2;
            if (rand < cumulative)
            {
                Debug.Log($"抽獎結果: {prize.Item1}");
                DetermineFinalReward(prize.Item1);
                return prize.Item1;
            }
        }

        return "ERROR"; // 應該不會發生，但加個保險
    }

    /// <summary>
    /// 決定最終獎勵
    /// </summary>
    private void DetermineFinalReward(string drawResult)
    {
        if (drawResult == "AA")
        {
            finalReward = "糰子";
            rewardSprites.Add(rewardDango);
        }
        else if (drawResult == "BB")
        {
            finalReward = "小氣球";
            rewardSprites.Add(rewardBalloon);
        }
        else
        {
            finalReward = "小郵票";
            rewardSprites.Add(rewardStamp);
        }

        Sprite selectedIcon = drawResult switch
        {
            "AA" => iconA,
            "BB" => iconB,
            "CC" => iconC,
            "DD" => iconD,
            "AB" => iconD,
            "BA" => iconD,
            "AC" => iconC,
            "CA" => iconC,
            "AD" => iconD,
            "DA" => iconD,
            "BC" => iconC,
            "CB" => iconC,
            "BD" => iconD,
            "DB" => iconD,
            "CD" => iconD,
            "DC" => iconD,
            _ => drawResult.Contains("C") ? iconC :
                 drawResult.Contains("D") ? iconD :
                 drawResult.Contains("A") ? iconA :
                 iconB
        };

        rewardImage.sprite = selectedIcon;
        rewardText.text = "你抽到了 圖案 " + drawResult;

        SaveToCloud(finalReward);
        OnRewardSelected?.Invoke(finalReward);
    }

    /// 存儲抽獎結果到雲端
    private void SaveToCloud(string reward)
    {
        PlayerPrefs.SetString("FinalReward", reward);
        PlayerPrefs.SetString("Timestamp", DateTime.Now.ToString());
        PlayerPrefs.Save();
        Debug.Log("已儲存獎勵: " + reward);
    }

    // 獲取所有儲存的獎勵
    public List<Sprite> GetRewards()
    {
        return rewardSprites;
    }
}