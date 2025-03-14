using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance;

    [Header("��� UI ����")]
     public GameObject gachaPanel;  // �������
    [SerializeField] private Image ticketImage;      // �ȱ��Ϥ�
    [SerializeField] private Image rewardImage;      // ��쪺���~�Ϥ�
    [SerializeField] private Text rewardText;        // ��쪺���~��r
    [SerializeField] private Button closeButton;     // �������s
    public GameObject resultScene; // ������G�e��

    [Header("�ʵe����")]
    [SerializeField] private GameObject paperAnim;  // ����ʵe

    [Header("���~�Ϯ�")]
    [SerializeField] private Sprite iconA; // A �Ϯ�
    [SerializeField] private Sprite iconB; // B �Ϯ�

    [Header("�̲׼��y")]
    [SerializeField] private Sprite rewardDango;   // �{�l
    [SerializeField] private Sprite rewardBalloon; // �p��y
    [SerializeField] private Sprite rewardStamp;   // �p�l��

    private string firstDrawResult = "";
    private string secondDrawResult = "";

    public bool isDragging = false;
    public bool ShowResult = false;

    public event Action<string> OnRewardSelected; // ����y��w��Ĳ�o
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
    /// Ĳ�o�������
    /// </summary>
    public void StartGacha() //�}�l����y�{
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

        // �]�w��쪺���~ (A �� B)
        string drawResult = Draw();

        yield return null;

        rewardImage.sprite = (drawResult == "A") ? iconA : iconB;
        rewardText.text = "�A���F " + (drawResult == "A" ? "�Ϯ� A" : "�Ϯ� B");

        // ���ݰʵe����

        // ��������e���A�}�ҵ��G�e��
        gachaPanel.SetActive(false);
        resultScene.SetActive(true);
        Debug.Log( $"�}�ҵ��G {drawResult}");

        // ���񵲪G�ʵe�]�p�G���^
        resultAnim = resultScene.GetComponent<Animator>();

    }

    public void CloseResult()
    {
        resultAnim.Play("ResultFadeOutAnim");
        OnNextLevel?.Invoke();
    }

    /// <summary>
    /// �H���� A �� B
    /// </summary>
    private string Draw()
    {
        int rand = UnityEngine.Random.Range(0,10);
        Debug.Log(rand);
        return (rand <= 5) ? "A" : "B";

    }

    /// <summary>
    /// �M�w�̲׼��y
    /// </summary>
    private void DetermineFinalReward()
    {
        string finalReward = "";
        Sprite rewardSprite = null;

        if (firstDrawResult == secondDrawResult)
        {
            if (firstDrawResult == "A")
            {
                finalReward = "�{�l";
                rewardSprite = rewardDango;
            }
            else
            {
                finalReward = "�p��y";
                rewardSprite = rewardBalloon;
            }
        }
        else
        {
            finalReward = "�p�l��";
            rewardSprite = rewardStamp;
        }

        SaveToCloud(finalReward);
        OnRewardSelected?.Invoke(finalReward);
    }

    /// <summary>
    /// �s�x������G�춳��
    /// </summary>
    private void SaveToCloud(string reward)
    {
        PlayerPrefs.SetString("FinalReward", reward);
        PlayerPrefs.SetString("Timestamp", DateTime.Now.ToString());
        PlayerPrefs.Save();
        Debug.Log("�w�x�s���y: " + reward);
    }
}
