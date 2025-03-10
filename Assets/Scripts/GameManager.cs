using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [SerializeField] private TransitionScreenDemo transDemo;

    private bool hasGameStart;
    private bool hasGameFinished;

    private LevelManager _levelManager;
    private int currentLevelIndex = 0; // 追蹤當前操作的 Level

    private Vector2Int startPos, endPos;
    private Vector3 curScreenPos;
    private bool isPressing = false; // 用來追蹤按壓狀態

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
        float yOffset = _levelManager.GetLevelPosition().y;
        startPos = new Vector2Int(Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.x)); // 修正 x 和 y 軸
        Debug.Log($"Touch Started at: {startPos}");
        _levelManager.HandleTouchStart(startPos);
    }

    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        if (!hasGameStart || hasGameFinished) return; // 確保遊戲已開始且未結束

        // 確保 _levelManager 已經被初始化
        if (_levelManager == null)
        {
            Debug.LogWarning("LevelManager 尚未被初始化！");
            return;
        }

        //curScreenPos = screenPos.ReadValue<Vector2>();
        //Vector3 worldPos = Camera.main.ScreenToWorldPoint(curScreenPos);
        //worldPos.z = 0;  // 確保 z 軸為 0

        // 修正座標計算，使其準確地對應到格子索引

        //float yOffset = _levelManager.GetLevelPosition().y;
        //endPos = new Vector2Int(Mathf.FloorToInt(worldPos.y - yOffset), Mathf.FloorToInt(worldPos.x));



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

        TransitionScreenManager transition = FindObjectOfType<TransitionScreenManager>(); // 找到場景中的 TransitionScreenManager
        transition.FinishedHideEvent += Initialize;
    }

    private void Initialize()
    {
        Debug.Log("Init");
        // 確保 parent 父物件開啟
        _parentContainer.gameObject.SetActive(true);
        if (_levelManager != null)
        {
            Debug.LogWarning("_levelManager 已經初始化，清除舊關卡");
            _levelManager.CleanUp();
        }

        hasGameFinished = false;
        hasGameStart = true;

        LoadLevel(currentLevelIndex);
    }

    private void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Count)
        {
            Debug.LogWarning("無效的關卡索引！");
            return;
        }


        ClearPreviousLevel();

        Level level = _levels[levelIndex];
        _levelManager = new LevelManager(level, _cellPrefab, _edgePrefab, _parentContainer);

        _levelManager.OnLevelComplete += HandleLevelComplete;
    }

    private void Update()
    {
        if (_levelManager != null && isPressing && hasGameStart)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos.ReadValue<Vector2>());

            // **修正座標轉換，減去 level.Position.y**
            float yOffset = _levelManager.GetLevelPosition().y;
            endPos = new Vector2Int(
                Mathf.FloorToInt(worldPos.y - yOffset), // Y 軸對應 Row
                Mathf.FloorToInt(worldPos.x) // X 軸對應 Col
            );

            Debug.Log($"滑鼠移動到: {endPos}");
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
            _levelManager.CleanUp();
            _levelManager = null;
        }

        yield return null;
        LoadLevel(currentLevelIndex);
    }

    private IEnumerator DelayedClear()
    {
        yield return new WaitForEndOfFrame();  // 等待一幀
        if (!isPressing)
        {
            Debug.Log("清除畫面");
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
            LoadLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log("All Levels Complete!");
            // 可以在這裡添加通關後的處理邏輯
        }
    }
}