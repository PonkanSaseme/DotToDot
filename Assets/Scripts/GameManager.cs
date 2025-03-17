using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 引入 UI 命名空間
using TransitionScreenPackage;
using TransitionScreenPackage.Demo;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private List<Level> _levels; // 多個 Level
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Transform _edgePrefab;
    [SerializeField] private Transform _parentContainer; // 最上層的父物件，管理所有關卡
    [SerializeField] public InputAction press, screenPos;

    [SerializeField] private GameObject transition; //轉場
    [SerializeField] private GameObject startScene; //開始畫面
    [SerializeField] private GameObject backgroundScene; //背景圖
    [SerializeField] private GameObject startIcon; // **拖入動畫 Image**
    [SerializeField] private GameObject _rulePrefab; //規則頁
    [SerializeField] private GameObject _finalResultScene; //最終結果頁


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

    private bool hasGamePass = false;

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

        GachaSystem.Instance.OnNextLevel -= GoToNextLevel;

        press.Disable();
        screenPos.Disable();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (!hasGameStart || hasGameFinished || hasGamePass) return; // 確保遊戲已開始且未結束

        // 確保 _levelManager 已經被初始化
        if (_levelManager == null)
        {
            Debug.LogWarning("LevelManager 尚未被初始化！");
            return;
        }
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
        if (!hasGameStart || hasGameFinished || hasGamePass) return; // 確保遊戲已開始且未結束

        // 確保 _levelManager 已經被初始化
        if (_levelManager == null)
        {
            return;
        }
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        isPressing = false;
        if (!hasGameStart || hasGameFinished||hasGamePass) return;

        StartCoroutine(DelayedClear());
    }

    public void StartGame()
    {
        startScene.SetActive(false);
        transDemo.enabled = true;

        TransitionScreenManager transition = FindObjectOfType<TransitionScreenManager>();
        // 確保不重複訂閱事件

        transition.FinishedRevealEvent += Initialize;
        GachaSystem.Instance.OnNextLevel += GoToNextLevel;
    }

    private void Initialize()
    {
        // 確保 parent 父物件開啟
        _parentContainer.gameObject.SetActive(true);
        if (_levelManager != null)
        {
            _levelManager.CleanUp();
        }

        _rulePrefab.SetActive(true);
    }

    public void OnRuleClick()
    {
        backgroundScene.SetActive(true);
        _rulePrefab.SetActive(false);
        hasGameFinished = false;
        hasGameStart = true;

        LoadLevel(currentLevelIndex);
    }

    private void LoadLevel(int levelIndex, bool redraw = false)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Count)
        {
            return;
        }

        isRedraw = redraw; // 設置標識
        isLevelTransitioning = false; // 重置標識
        hasGamePass = false;

        ClearPreviousLevel(); // 清理舊關卡

        Level level = _levels[levelIndex];
        _levelManager = new LevelManager(level, _cellPrefab, _edgePrefab, _parentContainer);

        _levelManager.OnLevelComplete += HandleLevelComplete;

        if (!isRedraw)
        {
            // 等待轉場動畫結束後再顯示 startIcon
            StartCoroutine(WaitForTransitionToEnd());
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
        hasGameFinished = true;
        hasGamePass = true;

        if (currentLevelIndex + 1 < _levels.Count)
        {
            StartCoroutine(RewardScene());
        }
        else
        {
            // 通關後的處理邏輯(第三關結束)
            _finalResultScene.SetActive(true);
            _finalResultScene.GetComponent<Animator>().Play("FinalResultAnim");

            // 顯示最終結果

            GachaSystem.Instance.FinalReward();

        }
    }

    public IEnumerator RewardScene()
    {
        currentLevelIndex++;

        yield return new WaitForSeconds(0.5f);

        // 抽獎系統
        GachaSystem.Instance.OpenGacha(currentLevelIndex);

    }

    private void GoToNextLevel()
    {

        if (currentLevelIndex + 1 <= _levels.Count)
        {
            if (currentLevelIndex == 1) {
                Level nextLevel = _levels[currentLevelIndex];
                float targetY = nextLevel.Position.y + nextLevel.Row / 2f; // 計算新關卡的目標 Y 座標
                
                CameraMover.Instance.MoveToNextLevel(targetY, () =>
                {
                    StartCoroutine(LoadNextLevelAfterDelay()); // 移動結束後載入新關卡
                });                                                           // 呼叫 CameraMover 來移動攝影機，並在移動完成後載入下一關
            } else if(currentLevelIndex == 2){
                Level nextLevel = _levels[currentLevelIndex];
                float targetY = nextLevel.Position.y + nextLevel.Row / 1f; // 計算新關卡的目標 Y 座標
                
                CameraMover.Instance.MoveToNextLevel(targetY, () =>
                {
                    StartCoroutine(LoadNextLevelAfterDelay()); // 移動結束後載入新關卡
                });                                                           // 呼叫 CameraMover 來移動攝影機，並在移動完成後載入下一關
            }

            GachaSystem.Instance.resultScene.SetActive(false);
        }
    }

    // 延遲載入下一關，確保移動過程不會瞬間跳過
    private IEnumerator LoadNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(0.3f); // 等待攝影機移動 + 停頓時間
        hasGameFinished = false;
        LoadLevel(currentLevelIndex, true); // 傳遞 true 來標識是重新繪製
    }

    private IEnumerator WaitForTransitionToEnd()
    {
        if (transDemo != null)
        {
            while (transDemo.IsTransitioning) // 確保轉場動畫正在播放
            {
                ShowStartIcon();
                yield return new WaitForSeconds(0.05f); // 等待下一幀
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

    public void HideStartIcon()
    {
        startIcon.SetActive(false);
    }
}