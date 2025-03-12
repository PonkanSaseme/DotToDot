using UnityEngine;
using UnityEngine.UI;

public class PrefabTagFetcher : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // ��J�A���w�s��
    [SerializeField] private Button button; // ��J�A�����s

    void Start()
    {
        if (prefab != null)
        {
            string prefabTag = prefab.tag;
            // �]�m���s�� onClick �ƥ�
            button.onClick.AddListener(() => OnButtonClick(prefab));
        }
    }

    void OnButtonClick(GameObject prefab)
    {
        // �N�w�s��]�m��������
        prefab.SetActive(false);
    }
}