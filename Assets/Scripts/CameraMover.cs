using System.Collections;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public static CameraMover Instance; // ��ҼҦ��A�T�O�u���@�� CameraMover

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
    /// ������v����U�@�����d����m
    /// </summary>
    /// <param name="targetY">�ؼ� Y �y��</param>
    /// <param name="callback">���ʧ����᪺�^�ը��</param>
    public void MoveToNextLevel(float targetY, System.Action callback)
    {
        if (isMoving)
        {
            Debug.LogWarning("��v�����b���ʤ�...");
            return;
        }

        StartCoroutine(MoveCameraCoroutine(targetY, callback));
    }

    private IEnumerator MoveCameraCoroutine(float targetY, System.Action callback)
    {
        isMoving = true;

        float duration = 3f; // ���ʫ���ɶ�
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, targetY, startPosition.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / duration); // ���Ʋ���
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        Camera.main.transform.position = targetPosition; // �T�O�̲צ�m�ǽT
        yield return new WaitForSeconds(0.5f); // ���� 0.5 ��A�T�O�e��í�w

        isMoving = false;

        callback?.Invoke(); // ���ʧ��������^�ը��
    }
}
