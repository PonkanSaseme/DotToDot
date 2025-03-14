using System.Collections;
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
    [SerializeField] private Text rewardText;        // 抽到的獎品文字
    [SerializeField] private Button closeButton;     // 關閉按鈕
    public GameObject resultScene; // 抽獎結果畫面

    [Header("動畫元件")]
    [SerializeField] private GameObject paperAnim;  // 抽獎動畫

    [Header("獎品圖案")]
    [SerializeField] private Sprite iconA; // A 圖案
    [SerializeField] private Sprite iconB; // B 圖案

    [Header("最終獎勵")]
    [SerializeField] private Sprite rewardDango;   // 糰子
    [SerializeField] private Sprite rewardBalloon; // 小氣球
    [SerializeField] private Sprite rewardStamp;   // 小郵票

    private string firstDrawResult = "";
    private string secondDrawResult = "";

    public bool isDragging = false;
    public bool ShowResult = false;

    public event Action<string> OnRewardSelected; // 當獎勵選定時觸發
    Animator paperControl;
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

        paperControl = paperAnim.GetComponent<Animator>();

        paperControl.Play("PaperDragAnim");

        StartCoroutine(CheckAniEnd());
    }

    private IEnumerator CheckAniEnd()
    {
        while (!paperControl.GetCurrentAnimatorStateInfo(0).IsName("paperFadeAnim")) 
        {
            yield return null;
        }
        while (paperControl.GetCurrentAnimatorStateInfo(0).normalizedTime<1)
        {
            yield return null;
        }

        Animator parentAnimator = gachaPanel.GetComponent<Animator>();
        parentAnimator.Play("GachaFadeOutAnim");

        while ( parentAnimator.GetCurrentAnimatorStateInfo(0).IsName("GachaFadeOutAnim") || parentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(ShowReward());
    }

    private IEnumerator ShowReward()
    {
        yield return new WaitForSeconds(0.5f);

        // 設定抽到的獎品 (A 或 B)
        string drawResult = Draw();

        yield return null;

        rewardImage.sprite = (drawResult == "A") ? iconA : iconB;
        rewardText.text = "你抽到了 " + (drawResult == "A" ? "圖案 A" : "圖案 B");

        // 等待動畫完成

        // 關閉抽獎畫面，開啟結果畫面
        gachaPanel.SetActive(false);
        resultScene.SetActive(true);
        Debug.Log( $"開啟結果 {drawResult}");

        // 播放結果動畫（如果有）
        resultAnim = resultScene.GetComponent<Animator>();

    }

    public void CloseResult()
    {
        resultAnim.Play("ResultFadeOutAnim");
        OnNextLevel?.Invoke();
    }

    /// <summary>
    /// 隨機抽 A 或 B
    /// </summary>
    private string Draw()
    {
        int rand = UnityEngine.Random.Range(0,10);
        Debug.Log(rand);
        return (rand <= 5) ? "A" : "B";

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
}
