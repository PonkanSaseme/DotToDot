using System.Collections;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public static CameraMover Instance; // 單例模式，確保只有一個 CameraMover

    private bool isMoving = false;

    [SerializeField] private Transform BackGround;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 移動攝影機到下一個關卡的位置
    /// </summary>
    /// <param name="targetY">目標 Y 座標</param>
    /// <param name="callback">移動完成後的回調函數</param>
    public void MoveToNextLevel(float targetY, System.Action callback)
    {
        if (isMoving)
        {
            Debug.LogWarning("攝影機正在移動中...");
            return;
        }

        StartCoroutine(MoveCameraCoroutine(targetY, callback));
    }

    private IEnumerator MoveCameraCoroutine(float targetY, System.Action callback)
    {
        isMoving = true;

        float duration = 3f; // 移動持續時間
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, targetY, startPosition.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / duration); // 平滑移動
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        Camera.main.transform.position = targetPosition; // 確保最終位置準確
        yield return new WaitForSeconds(0.5f); // 等待 0.5 秒，確保畫面穩定

        isMoving = false;

        callback?.Invoke(); // 移動完成後執行回調函數
    }
}
