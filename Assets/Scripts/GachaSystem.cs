using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net;
using System.Text;
using TMPro;

public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance;

    [Header("抽獎 UI 元件")]
    public GameObject gachaPanel;  // 抽獎視窗
    [SerializeField] private Image ticketImage;      // 紙條圖片
    [SerializeField] private Image rewardImage;      // 抽到的獎品圖片
    [SerializeField] public Image finalRewardImage; // 最終獎品圖片

    [SerializeField] private TextMeshProUGUI rewardText; // 抽到的獎品文字
    [SerializeField] public TextMeshProUGUI finalRewardText; // 最終獎品文字
    [SerializeField] public Image firstRewardImage;  // 第一個獎勵圖片
    [SerializeField] public Image secondRewardImage; // 第二個獎勵圖片
    [SerializeField] public TextMeshProUGUI firstRewardText;  // 第一個獎勵文字
    [SerializeField] public TextMeshProUGUI secondRewardText; // 第二個獎勵文字

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

    [Header("紀錄資訊")]
    [SerializeField] private TextMeshProUGUI timeStamp;   // 時間戳記
    [SerializeField] private TMP_InputField enterID;   // 玩家輸入ID

    [Header("機率用項目")]
    private float A_rate = 0.01f;
    private float B_rate = 0.07f;




    public event Action<string> OnRewardSelected; // 當獎勵選定時觸發
    Animator parentAnimator;
    Animator resultAnim;

    private int levelnum = 0;

    public event Action OnNextLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        gachaPanel.SetActive(false);
        PlayerPrefs.DeleteAll();
    }

    public void OpenGacha(int index)
    {
        gachaPanel.SetActive(true);
        levelnum = index;
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
        StartCoroutine(ShowReward()); //開始跑抽獎
        
    }

    private IEnumerator ShowReward()
    {
        yield return new WaitForSeconds(0.5f);

        // 設定抽到的最終獎品
        string drawResult = GetRandomPrize(); //回傳抽出來結果

        PlayerPrefs.SetString($"level{levelnum}",drawResult);

        (rewardImage.sprite, rewardText.text) = GetReward($"level{levelnum}");

        yield return null;

        // 關閉抽獎畫面，開啟結果畫面
        gachaPanel.SetActive(false);
        resultScene.SetActive(true);

        // 播放結果動畫（如果有）
        resultAnim = resultScene.GetComponent<Animator>();
    }

    public void CloseResult()
    {
        resultAnim.Play("ResultFadeOutAnim");
        OnNextLevel?.Invoke();
    }

    public (Sprite,string) GetReward(string levelnum)
    {
        Sprite sprite=null;
        string rewardtext = string.Empty;
        switch (PlayerPrefs.GetString(levelnum))
        {
            case "A":
                sprite = iconA;
                rewardtext = "糰子";
                break;
            case "B":
                sprite = iconB;
                rewardtext = "小氣球";
                break;
            case "C":
                sprite = iconC;
                rewardtext = "小郵票";
                break;
        }

        return (sprite, rewardtext);
    }


    //跑抽獎機率
    private string GetRandomPrize()
    {
        float rand = UnityEngine.Random.value; // 取得 0~1 之間的隨機數
        if (rand < A_rate) return "A";
        if (rand < A_rate + B_rate) return "B";
        return "C";
    }

    /// 決定最終獎勵
    public (Sprite,string) GetFinalReward()
    {
        string finalText = string.Empty;
        Sprite sprite = null;

        string drawResult = PlayerPrefs.GetString("level1") + PlayerPrefs.GetString("level2");
        
        switch (drawResult)
        {
            case "AA":
                finalText = "糰子";
                sprite = iconA;
                break;
            case "BB":
                finalText = "小氣球";
                sprite = iconB;
                break;
            default:
                finalText = "小郵票";
                sprite = iconC;
                break;
        }
        PlayerPrefs.SetString("FinalReward", finalText);
        return (sprite,finalText);
    }

    public void FinalReward()
    {
        (firstRewardImage.sprite, firstRewardText.text) = GetReward("level1");
        (secondRewardImage.sprite, secondRewardText.text) = GetReward("level2");
        (finalRewardImage.sprite, finalRewardText.text) = GetFinalReward();
        SaveData();
    }

    /// 存儲抽獎輸出到記事本
    public void SaveData()
    {
        
        PlayerPrefs.SetString("Timestamp", DateTime.Now.ToString());
        PlayerPrefs.Save();

        timeStamp.text = DateTime.Now.ToString();
    }

    /// <summary>
    /// 將獎勵資訊寫入 txt 檔案
    /// </summary>
    public void SaveToFile()
    {
        FeedbackForm.Instance.SubmitFeedback();
    }

    private IEnumerator SendToGoogleSheetUsingHttpWebRequest()
    {
        string googleScriptUrl = "https://script.google.com/macros/s/AKfycbwBCqhN_11aFJvlldC6VIBqToBVsd4tPbDFmFR0dwwUPdV558W1ny4h2trni-wNX6siCw/exec"; // 替換成你部署的 Apps Script URL
        string postData = $"id={enterID.text}&reward={PlayerPrefs.GetString("FinalReward")}";

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(googleScriptUrl);
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";

        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentLength = byteArray.Length;

        using (Stream dataStream = request.GetRequestStream())
        {
            dataStream.Write(byteArray, 0, byteArray.Length);
        }

        WebResponse response = null;
        try
        {
            response = request.GetResponse();
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream);
                string responseFromServer = reader.ReadToEnd();
                Debug.Log("成功發送到 Google Sheets: " + responseFromServer);
            }
        }
        catch (WebException e)
        {
            using (WebResponse errResponse = e.Response)
            {
                using (Stream errData = errResponse.GetResponseStream())
                {
                    string text = new StreamReader(errData).ReadToEnd();
                    Debug.LogError("發送失敗: " + e.Message);
                    Debug.LogError("回應訊息: " + text);
                }
            }
        }
        finally
        {
            if (response != null)
            {
                response.Close();
            }
        }

        yield return null;
    }
}