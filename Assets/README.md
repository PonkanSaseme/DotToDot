# DotToDot
=============
Edmond's Birthday event small game.
專案名稱: DotToDot
Unity 版本: 2021.3.17f1
開發者: [天柑地芝]

> 本專案是一款 Dot to Dot 連線遊戲，玩家透過拖曳方式在方格內畫線，連接起點與終點，並需經過所有可通行的格子才能完成關卡。
目前支援 多關卡 (3 個以上)，並允許 電腦 (滑鼠) 與 手機 (觸控) 進行操作。
=============

<table> DotToDotProject/
<ol> 📁 Assets/
│    <il> 📁 Scripts/              # 主要 C# 程式碼
│    │    <p> GameManager.cs      # 控制遊戲主流程，管理關卡與玩家輸入</p>
│    │    <p> Level.cs            # ScriptableObject，儲存關卡數據</p>
│    │    <p>Cell.cs             # 單個格子的邏輯控制</p>
│    │    <p> TransitionScreenManager.cs  # 控制轉場動畫</p>
│    │    <p> InputManager.cs     # (可選) 若獨立管理 Input System</p>
│    │
     </il>
│    <li>📁 Prefabs/              # 預製物件
│    │    <p> Cell.prefab         # 格子物件</p>
│    │    <p> Edge.prefab         # 連線線條</p>
│    │    <p> ParentContainer.prefab # 管理所有關卡的父物件</p>
│    </li>
│    <li> 📁 Levels/               # 儲存 ScriptableObject 關卡資料
│    │    <p> Level1.asset</p>
│    │    ├── Level2.asset
│    │    ├── Level3.asset
│    </li>
│    ├── 📁 UI/                   # 遊戲 UI 相關
│    │    ├── StartButton.prefab  # 開始遊戲按鈕
│    │    ├── WinScreen.prefab    # 通關畫面
│    │
│    ├── 📁 Materials/            # 材質與顏色
│    │    ├── CellMaterial.mat
│    │    ├── EdgeMaterial.mat
│    │
│    ├── 📁 Scenes/               # Unity 場景
│    │    ├── MainMenu.unity      # 主選單
│    │    ├── GameScene.unity     # 遊戲主場景
│
│── 📁 Packages/                   # Unity Packages (Input System, URP 等)
│── 📁 ProjectSettings/             # 專案設定檔
│── 📁 UserSettings/
README.md                       # 專案說明文件 </ol>
</table>


# 遊戲玩法
1.點擊 Start 按鈕 開始遊戲。
2.滑鼠左鍵 (PC) 或 手指觸控 (手機) 拖曳來畫線。
3.線條必須從 StartPosition 開始，並連接到 EndPosition。
4.必須經過所有可通行的格子 才能成功通關。
5.移動時不可穿越障礙物 (無法通行的格子)。
6.通關後顯示 "Game Clear!" 訊息，可點擊按鈕返回主選單或進入下一關。

## 技術細節
1.GameManager (遊戲核心邏輯)
管理所有關卡 (_levels)，允許同時顯示 3 個關卡，透過 List<Level> 控制。
使用 cellsList、edgesList、filledPointsList 管理不同關卡的數據。
支援 Input System，允許 滑鼠點擊/拖曳 和 手機觸控 進行畫線。
計算玩家目前在哪個 level，並確保玩家只能在 StartPosition 開始連線。
動態調整攝影機 (AdjustCamera())，確保所有關卡都能完整顯示。

2.Level (關卡數據)
透過 ScriptableObject 儲存關卡配置 (Row, Col, StartPosition, EndPosition 等)。
GridData 儲存地圖格子 (可通行 / 障礙物)，並使用 一維陣列 (List<bool>) 來存取資料。
提供 GetCell(row, col) 方法，讓 GameManager 能正確讀取關卡資訊。

3.Cell (格子)
管理單個格子的狀態 (Blocked, Filled)，並更新顏色顯示。
Init(isWalkable) 會設定格子的 可通行狀態，並調整顏色 (_blockedColor / _emptyColor)。
SetStartColor() 和 SetEndColor() 設定特殊起點/終點顏色。

4.Input System (輸入管理)
PC: 滑鼠點擊 & 拖曳 (press & screenPos)
Mobile: 手指觸控 (press & screenPos)
透過 OnTouchStarted()、OnTouchPerformed()、OnTouchCanceled() 來偵測玩家輸入。

### 安裝與執行
1.安裝 Unity 2021.3.17f1
2.開啟專案 (File > Open Project)
3.開啟 GameScene.unity
4.點擊 ▶ Play 測試遊戲

