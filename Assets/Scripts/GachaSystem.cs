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
    [SerializeField] private Image finalRewardImage; // �̲׼��~�Ϥ�
    [SerializeField] private Text finalRewardText; // �̲׼��~��r
    [SerializeField] private Text rewardText;        // ��쪺���~��r
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

    string finalReward = "";
    Sprite rewardSprite = null;

    public event Action<string> OnRewardSelected; // ����y��w��Ĳ�o
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
        while (parentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime<1)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        StartCoroutine(ShowReward());
    }

    private IEnumerator ShowReward()
    {
        yield return new WaitForSeconds(0.5f);

        // �]�w��쪺�̲׼��~
        string drawResult = Draw();

        yield return null;

        DetermineFinalReward(drawResult); //�̲׵��G���~

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

    //������v�p��
    public string Draw()
    {
        // �w�q�����M���������v
        (string, float)[] prizes = new (string, float)[]
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

        float rand = UnityEngine.Random.value; // ���o 0~1 �������H����
        float cumulative = 0f;

        foreach (var prize in prizes)
        {
            cumulative += prize.Item2;
            if (rand < cumulative)
            {
                Debug.Log($"������G: {prize.Item1}");
                return prize.Item1;
            }
        }

        return "ERROR"; // ���Ӥ��|�o�͡A���[�ӫO�I
    }

    /// <summary>
    /// �M�w�̲׼��y
    /// </summary>
    private void DetermineFinalReward(string drawResult)
    {
        if (drawResult == "AA")
        {
            finalReward = "�{�l";
            rewardSprite = rewardDango;
        }
        else if (drawResult == "BB")
        {
            finalReward = "�p��y";
            rewardSprite = rewardBalloon;
        }
        else
        {
            finalReward = "�p�l��";
            rewardSprite = rewardStamp;
        }

        Sprite selectedIcon = drawResult switch
        {
            "AA" => iconA,
            "BB" => iconB,
            "CC" => iconC,
            "DD" => iconD,
            _ => (drawResult[0] == 'A') ? iconA :
                 (drawResult[0] == 'B') ? iconB :
                 (drawResult[0] == 'C') ? iconC :
                 iconD
        };

        rewardImage.sprite = selectedIcon;
        rewardText.text = "�A���F �Ϯ� " + drawResult;

        SaveToCloud(finalReward);
        OnRewardSelected?.Invoke(finalReward);
    }


    /// �s�x������G�춳��
    private void SaveToCloud(string reward)
    {
        PlayerPrefs.SetString("FinalReward", reward);
        PlayerPrefs.SetString("Timestamp", DateTime.Now.ToString());
        PlayerPrefs.Save();
        Debug.Log("�w�x�s���y: " + reward);
    }
}
