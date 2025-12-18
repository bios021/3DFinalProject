using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 如果你有倒數計時的 UI

public class GameLevelController : MonoBehaviour
{
    [Header("遊戲規則設定")]
    public float levelTime = 18f; // 倒數 3 分鐘
    public TMP_Text timerText;     // 拖入顯示時間的 UI (可選)

    [Header("狀態監控 (唯讀)")]
    public int totalBeans;         // 總糖豆數
    public int currentProcessed;   // 已處理 (死亡+獲救)
    public int savedCount;         // 成功救出的數量

    private float currentTimer;
    private bool isGameOver = false;

    void Start()
    {
        currentTimer = levelTime;

        // 自動算出場景裡有多少個標籤為 "Bean" 的物件
        GameObject[] beans = GameObject.FindGameObjectsWithTag("NormalBean");
        totalBeans = beans.Length;

        Debug.Log($"遊戲開始：限時 {levelTime} 秒，共有 {totalBeans} 個糖豆");
    }

    void Update()
    {
        if (isGameOver) return;

        // 1. 倒數計時邏輯
        currentTimer -= Time.deltaTime;

        // 更新 UI (如果有)
        if (timerText != null)
        {
            int totalSeconds = Mathf.FloorToInt(currentTimer);

            // 2. 換算分鐘與秒數
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // 3. 更新 UI，格式化成 00:00 (冒號前的00是分，冒號後的00是秒)
            if (timerText != null)
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // 條件 A: 時間結束
        if (currentTimer <= 0)
        {
            EndGame(false, "Time Out"); // 失敗：時間到
        }
    }

    // ================= 外部呼叫區 =================

    // 條件 B: 玩家死亡 (由 Player 呼叫)
    public void OnPlayerDied()
    {
        if (isGameOver) return;
        EndGame(false, "You Died"); // 失敗：玩家死亡
    }

    // 條件 C: 糖豆狀態更新 (由 Bean 呼叫)
    public void OnBeanStateChanged(bool isSaved)
    {
        if (isGameOver) return;

        currentProcessed++; // 處理數量 +1

        if (isSaved) savedCount++; // 如果是獲救，積分 +1

        Debug.Log($"進度: {currentProcessed}/{totalBeans}");

        // 檢查是否所有糖豆都處理完了
        if (currentProcessed >= totalBeans)
        {
            EndGame(true, "Mission Clear"); // 結束：全部結算完畢
        }
    }

    // ================= 結算處理 =================
    void EndGame(bool win, string reason)
    {
        isGameOver = true;
        Debug.Log($"遊戲結束 - 結果: {win}, 原因: {reason}");

        // 存檔資料傳給下一關
        GameData.isWin = win;
        GameData.failReason = reason;
        GameData.score = savedCount;

        // 載入結算場景 (請確認場景名稱是否正確)
        SceneManager.LoadScene("result"); // 注意大小寫要跟 Build Settings 一樣
    }
}