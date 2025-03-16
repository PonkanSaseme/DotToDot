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

    [Header("��� UI ����")]
    public GameObject gachaPanel;  // �������
    [SerializeField] private Image ticketImage;      // �ȱ��Ϥ�
    [SerializeField] private Image rewardImage;      // ��쪺���~�Ϥ�
    [SerializeField] public Image finalRewardImage; // �̲׼��~�Ϥ�

    [SerializeField] private TextMeshProUGUI rewardText; // ��쪺���~��r
    [SerializeField] public TextMeshProUGUI finalRewardText; // �̲׼��~��r
    [SerializeField] public Image firstRewardImage;  // �Ĥ@�Ӽ��y�Ϥ�
    [SerializeField] public Image secondRewardImage; // �ĤG�Ӽ��y�Ϥ�
    [SerializeField] public TextMeshProUGUI firstRewardText;  // �Ĥ@�Ӽ��y��r
    [SerializeField] public TextMeshProUGUI secondRewardText; // �ĤG�Ӽ��y��r

    [SerializeField] private Button closeButton;     // �������s
    public GameObject resultScene; // ������G�e��

    [Header("�ʵe����")]
    [SerializeField] private GameObject paperAnim;  // ����ʵe

    [Header("���~�Ϯ�")]
    [SerializeField] private Sprite iconA; // A �Ϯ�
    [SerializeField] private Sprite iconB; // B �Ϯ�
    [SerializeField] private Sprite iconC; // C �Ϯ� (���´f�U)
    [SerializeField] private Sprite iconD; // D �Ϯ� (���´f�U)

    [Header("�̲׼��y")]
    [SerializeField] private Sprite rewardDango;   // �{�l
    [SerializeField] private Sprite rewardBalloon; // �p��y
    [SerializeField] private Sprite rewardStamp;   // �p�l��

    [Header("������T")]
    [SerializeField] private TextMeshProUGUI timeStamp;   // �ɶ��W�O
    [SerializeField] private TMP_InputField enterID;   // ���a��JID

    [Header("���v�ζ���")]
    private float A_rate = 0.01f;
    private float B_rate = 0.07f;




    public event Action<string> OnRewardSelected; // ����y��w��Ĳ�o
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
    /// Ĳ�o�������
    /// </summary>
    public void StartGacha() //�}�l����y�{
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
        StartCoroutine(ShowReward()); //�}�l�]���
        
    }

    private IEnumerator ShowReward()
    {
        yield return new WaitForSeconds(0.5f);

        // �]�w��쪺�̲׼��~
        string drawResult = GetRandomPrize(); //�^�ǩ�X�ӵ��G

        PlayerPrefs.SetString($"level{levelnum}",drawResult);

        (rewardImage.sprite, rewardText.text) = GetReward($"level{levelnum}");

        yield return null;

        // ��������e���A�}�ҵ��G�e��
        gachaPanel.SetActive(false);
        resultScene.SetActive(true);

        // ���񵲪G�ʵe�]�p�G���^
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
                rewardtext = "�{�l";
                break;
            case "B":
                sprite = iconB;
                rewardtext = "�p��y";
                break;
            case "C":
                sprite = iconC;
                rewardtext = "�p�l��";
                break;
        }

        return (sprite, rewardtext);
    }


    //�]������v
    private string GetRandomPrize()
    {
        float rand = UnityEngine.Random.value; // ���o 0~1 �������H����
        if (rand < A_rate) return "A";
        if (rand < A_rate + B_rate) return "B";
        return "C";
    }

    /// �M�w�̲׼��y
    public (Sprite,string) GetFinalReward()
    {
        string finalText = string.Empty;
        Sprite sprite = null;

        string drawResult = PlayerPrefs.GetString("level1") + PlayerPrefs.GetString("level2");
        
        switch (drawResult)
        {
            case "AA":
                finalText = "�{�l";
                sprite = iconA;
                break;
            case "BB":
                finalText = "�p��y";
                sprite = iconB;
                break;
            default:
                finalText = "�p�l��";
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

    /// �s�x�����X��O�ƥ�
    public void SaveData()
    {
        
        PlayerPrefs.SetString("Timestamp", DateTime.Now.ToString());
        PlayerPrefs.Save();

        timeStamp.text = DateTime.Now.ToString();
    }

    /// <summary>
    /// �N���y��T�g�J txt �ɮ�
    /// </summary>
    public void SaveToFile()
    {
        FeedbackForm.Instance.SubmitFeedback();
    }

    private IEnumerator SendToGoogleSheetUsingHttpWebRequest()
    {
        string googleScriptUrl = "https://script.google.com/macros/s/AKfycbwBCqhN_11aFJvlldC6VIBqToBVsd4tPbDFmFR0dwwUPdV558W1ny4h2trni-wNX6siCw/exec"; // �������A���p�� Apps Script URL
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
                Debug.Log("���\�o�e�� Google Sheets: " + responseFromServer);
            }
        }
        catch (WebException e)
        {
            using (WebResponse errResponse = e.Response)
            {
                using (Stream errData = errResponse.GetResponseStream())
                {
                    string text = new StreamReader(errData).ReadToEnd();
                    Debug.LogError("�o�e����: " + e.Message);
                    Debug.LogError("�^���T��: " + text);
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