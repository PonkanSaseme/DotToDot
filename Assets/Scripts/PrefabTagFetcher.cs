using UnityEngine;
using UnityEngine.UI;

public class PrefabTagFetcher : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // 拖入你的預製件
    [SerializeField] private Button button; // 拖入你的按鈕

    void Start()
    {
        if (prefab != null)
        {
            string prefabTag = prefab.tag;
            // 設置按鈕的 onClick 事件
            button.onClick.AddListener(() => OnButtonClick(prefab));
        }
    }

    void OnButtonClick(GameObject prefab)
    {
        // 將預製件設置為不活動
        prefab.SetActive(false);
    }
}