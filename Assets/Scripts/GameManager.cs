using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 引入 UI 命名空間
using TransitionScreenPackage;
using TransitionScreenPackage.Demo;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private List<Level> _levels; // 多個 Level
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Transform _edgePrefab;
    [SerializeField] private Transform _parentContainer; // 最上層的父物件，管理所有關卡
    [SerializeField] private InputAction press, screenPos;

    [SerializeField] private GameObject transition; //轉場
    [SerializeField] private GameObject startScene; //開始畫面
    [SerializeField] private GameObject startIcon; // **拖入動畫 Image**

    [SerializeField] private TransitionScreenDemo transDemo;

    private bool isCameraMoving = false; //確保移動過程中不會打斷

    private bool hasGameStart;
    private bool hasGameFinished;

    private LevelManager _levelManager;
    private int currentLevelIndex = 0; // 追蹤當前操作的 Level

    private Vector2Int startPos, endPos;
    private Vector3 curScreenPos;
    private bool isPressing = false; // 用來追蹤按壓狀態

    private bool isRedraw = false;
    private bool isLevelTransitioning = false; // 新增標識

    private void Awake()
    {
        Instance = this;
        hasGameStart = false;
    }

    private void OnEnable()
    {
        // 訂閱輸入事件
        press.Enable();
        screenPos.Enable();

        // 按下滑鼠/觸控開始
        press.started += OnTouchStarted;
        // 持續拖曳時
        press.performed += OnTouchPerformed;
        // 放開滑鼠/觸控結束
        press.canceled += OnTouchCanceled;
    }

    private void OnDisable()
    {
        // 取消訂閱，避免記憶體洩漏
        press.started -= OnTouchStarted;
        press.performed -= OnTouchPerformed;
        press.canceled -= OnTouchCanceled;
        press.Disable();
        screenPos.Disable();

    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (!hasGameStart || hasGameFinished) return; // 確保遊戲已開始且未結束

        // 確保 _levelManager 已經被初始化
        if (_levelManager == null) return;
        isPressing = true;
        curScreenPos = screenPos.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(curScreenPos);
        worldPos.z = 0;  // 確保 z 軸為 0

        // 修正座標計算，使其準確地對應到格子索引
        startPos = new Vector2Int(Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.x)); // 修正 x 和 y 軸
        _levelManager.HandleTouchStart(startPos);
    }

    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        if (!hasGameStart || hasGameFinished) return; // 確保遊戲已開始且未結束

        // 確保 _levelManager 已經被初始化
        if (_levelManager == null) return;
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        isPressing = false;
        StartCoroutine(DelayedClear());
    }

    public void StartGame()
    {
        startScene.SetActive(false);
        transDemo.enabled = true;

        TransitionScreenManager transition = FindObjectOfType<TransitionScreenManager>();

        // 確保不重複訂閱事件
        transition.FinishedRuleEvent -= OnTransitionFinished;
        transition.FinishedRuleEvent += OnTransitionFinished;

        transition.FinishedRuleEvent += Initialize;
    }

    private void Initialize()
    {
        Debug.Log("Init");
        // 確保 parent 父物件開啟
        _parentContainer.gameObject.SetActive(true);
        Debug.Log("Parent Container Active: " + _parentContainer.gameObject.activeSelf);
        if (_levelManager != null)
        {
            _levelManager.CleanUp();
        }

        hasGameFinished = false;
        hasGameStart = true;

        LoadLevel(currentLevelIndex);
    }

    private void LoadLevel(int levelIndex, bool redraw = false)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Count)
        {
            Debug.LogWarning("無效的關卡索引！");
            return;
        }

        isRedraw = redraw; // 設置標識
        isLevelTransitioning = false; // 重置標識

        ClearPreviousLevel(); // 清理舊關卡

        Level level = _levels[levelIndex];
        _levelManager = new LevelManager(level, _cellPrefab, _edgePrefab, _parentContainer);

        _levelManager.OnLevelComplete += HandleLevelComplete;

        if (!isRedraw)
        {
            _levelManager.FadeInLevel(); // 開始淡入動畫
            // 等待轉場動畫結束後再顯示 startIcon
            StartCoroutine(WaitForTransitionToEnd());
        }
        else
        {
            ShowStartIcon();
        }
    }

    private void Update()
    {
        if (_levelManager != null && isPressing && hasGameStart)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos.ReadValue<Vector2>());

            // 修正座標轉換
            endPos = new Vector2Int(
                Mathf.FloorToInt(worldPos.y), // Y 軸對應 Row
                Mathf.FloorToInt(worldPos.x) // X 軸對應 Col
            );

            _levelManager.HandleTouchMove(endPos);
        }
    }

    private void OnTransitionFinished()
    {
        Debug.Log("OnTransitionFinished Called");
        if (!transDemo.IsTransitioning && _levelManager != null)
        {
            if (!isLevelTransitioning) // 檢查是否在關卡轉場中
            {
                _levelManager.FadeInLevel(); //讓 LevelManager 啟動淡入
            }
        }
    }



    private void ClearPreviousLevel()
    {
        // 清理之前的關卡物件
        foreach (Transform child in _parentContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator ClearData()
    {
        hasGameFinished = false;

        if (_levelManager != null)
        {
            _levelManager.OnLevelComplete -= HandleLevelComplete;
            HideStartIcon(); // 清除 Start Icon
            _levelManager.CleanUp();
            _levelManager = null;
        }

        yield return null;
        LoadLevel(currentLevelIndex, true); // 傳遞 true 來標識是重新繪製
    }

    private IEnumerator DelayedClear()
    {
        yield return new WaitForEndOfFrame();  // 等待一幀
        if (!isPressing)
        {
            StartCoroutine(ClearData());
        }
    }

    private void HandleLevelComplete()
    {
        Debug.Log("Level Complete!");
        hasGameFinished = true;

        if (currentLevelIndex + 1 < _levels.Count)
        {
            currentLevelIndex++;
            StartCoroutine(MoveCameraToNextLevel()); // 確保攝影機移動
            StartCoroutine(LoadNextLevelAfterDelay()); // 等待攝影機移動完成後載入
        }
        else
        {
            Debug.Log("All Levels Complete!");
            // 可以在這裡添加通關後的處理邏輯
        }
    }

    public IEnumerator FadeInCoroutine(SpriteRenderer[] sprites, float duration)
    {
        float elapsedTime = 0f;

        // 記錄每個 Sprite 原始透明度
        Dictionary<SpriteRenderer, float> originalAlphas = new Dictionary<SpriteRenderer, float>();

        foreach (var sprite in sprites)
        {
            if (sprite == null) continue; // 檢查對象是否存在
            originalAlphas[sprite] = sprite.color.a; // 紀錄原始透明度
            Color tempColor = sprite.color;
            tempColor.a = 0;  // 先把所有物件設為透明
            sprite.color = tempColor;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alphaLerp = Mathf.Lerp(0, 1, elapsedTime / duration);

            foreach (var sprite in sprites)
            {
                if (sprite == null) continue; // 檢查對象是否存在
                Color tempColor = sprite.color;

                // 原本 alpha = 1 的物件才會從 0 淡入
                if (originalAlphas[sprite] == 1)
                {
                    tempColor.a = alphaLerp;
                }
                else
                {
                    tempColor.a = 0; // 原本是 0 的保持透明
                }

                sprite.color = tempColor;
            }
            yield return null;
        }

        // 確保最終透明度正確
        foreach (var sprite in sprites)
        {
            if (sprite == null) continue; // 檢查對象是否存在
            Color tempColor = sprite.color;
            tempColor.a = originalAlphas[sprite] == 1 ? 1 : 0; // 原本是 1 的變回 1，0 的保持 0
            sprite.color = tempColor;
        }

    }

    // 延遲載入下一關，確保移動過程不會瞬間跳過
    private IEnumerator LoadNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(3.5f); // 等待攝影機移動 + 停頓時間
        LoadLevel(currentLevelIndex, true); // 傳遞 true 來標識是重新繪製
    }

    private IEnumerator MoveCameraToNextLevel()
    {
        if (currentLevelIndex + 1 >= _levels.Count || isCameraMoving)
            yield break; // 如果已經是最後一關，或攝影機正在移動，就不執行

        isCameraMoving = true; // 標記攝影機正在移動
        isLevelTransitioning = true; // 標記關卡轉場

        Level nextLevel = _levels[currentLevelIndex + 1];
        float targetY = nextLevel.Position.y + nextLevel.Row / 2f; // 確保新關卡在畫面正中央

        float duration = 3f; // 延長時間，讓移動更平滑
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, targetY, startPosition.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / duration); // 使用 SmoothStep 確保平滑
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        Camera.main.transform.position = targetPosition; // 確保最終位置正確

        yield return new WaitForSeconds(0.5f); // 等待 0.5 秒，確保畫面不會馬上切換

        isCameraMoving = false; // 攝影機移動結束
        isLevelTransitioning = false; // 標記關卡轉場結束
    }

    private IEnumerator WaitForTransitionToEnd()
    {
        if (transDemo != null)
        {
            while (transDemo.IsTransitioning) // 確保轉場動畫正在播放
            {
                yield return null; // 等待下一幀
            }
        }

    }

    private void ShowStartIcon()
    {
        if (startIcon != null && _levelManager != null)
        {
            Vector3 startPosition = _levelManager.GetStartIconPosition();
            startIcon.transform.position = startPosition;
            startIcon.SetActive(true);
        }
    }

    private void HideStartIcon()
    {
        if (startIcon != null)
        {
            startIcon.SetActive(false);
        }
    }
}