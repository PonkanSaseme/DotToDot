using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance;

    [Header("��� UI ����")]
    [SerializeField] private GameObject gachaPanel;  // �������
    [SerializeField] private Image ticketImage;      // �ȱ��Ϥ�
    [SerializeField] private Image rewardImage;      // ��쪺���~�Ϥ�
    [SerializeField] private Text rewardText;        // ��쪺���~��r
    [SerializeField] private Button closeButton;     // �������s

    [Header("�즲����")]
    [SerializeField] private RectTransform ticketTransform; // �ȱ� RectTransform
    [SerializeField] private float dragThreshold = 150f; // �h�ֶZ���������\�즲

    [Header("���~�Ϯ�")]
    [SerializeField] private Sprite iconA; // A �Ϯ�
    [SerializeField] private Sprite iconB; // B �Ϯ�

    [Header("�̲׼��y")]
    [SerializeField] private Sprite rewardDango;   // �{�l
    [SerializeField] private Sprite rewardBalloon; // �p��y
    [SerializeField] private Sprite rewardStamp;   // �p�l��

    private string firstDrawResult = "";
    private string secondDrawResult = "";

    private bool isDragging = false;
    private Vector2 startPos;

    public event Action<string> OnRewardSelected; // ����y��w��Ĳ�o

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
    /// Ĳ�o�������
    /// </summary>
    public void StartGacha()
    {
        gachaPanel.SetActive(true);
        ticketTransform.anchoredPosition = startPos; // ���m�ȱ���m
        rewardImage.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// �B�z�ȱ��즲
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

        // �]�w��쪺���~ (A �� B)
        string drawResult = Draw();
        rewardImage.sprite = (drawResult == "A") ? iconA : iconB;
        rewardText.text = "�A���F " + (drawResult == "A" ? "�Ϯ� A" : "�Ϯ� B");

        rewardImage.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);

        // �O�����G
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
    /// �H���� A �� B
    /// </summary>
    private string Draw()
    {
        float rand = UnityEngine.Random.value;
        return (rand <= 0.5f) ? "A" : "B"; // 50% ���v
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

    /// <summary>
    /// �����������
    /// </summary>
    private void CloseGacha()
    {
        gachaPanel.SetActive(false);
    }
}
