using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultManager : MonoBehaviour
{
    [Header("UI 文字組件")]
    public TextMeshProUGUI savedCountText;  // 顯示拯救數量
    public TextMeshProUGUI timeText;        // 顯示耗時
    public TextMeshProUGUI statusText;      // 顯示存活狀態
    public TextMeshProUGUI ratingText;      // 顯示評分 (S, A, B...)

    [Header("設定")]
    public string gameSceneName = "GameScene"; // 重新開始要讀取的場景
    public string menuSceneName = "IntroScene"; // 回主選單

    void Start()
    {
        // 1. 顯示基本數據
        ShowResults();

        // 2. 計算並顯示評分
        CalculateAndShowRating();
    }

    void ShowResults()
    {
        // 顯示拯救數量
        savedCountText.text = "save count: " + GameData.savedCount ;

        // 顯示耗時 (把秒數轉成 00:00 格式)
        int minutes = Mathf.FloorToInt(GameData.timeSpent / 60F);
        int seconds = Mathf.FloorToInt(GameData.timeSpent % 60F);
        timeText.text = string.Format("time: {0:00}:{1:00}", minutes, seconds);

        // 顯示狀態
        if (GameData.isPlayerAlive)
        {
            statusText.text = "status: <color=green>生還</color>";
        }
        else
        {
            statusText.text = "status: <color=red>死亡</color>";
        }
    }

    void CalculateAndShowRating()
    {
        string grade = "F";
        Color gradeColor = Color.gray;

        // --- 評分邏輯 (你可以根據自己的遊戲難度調整) ---

        if (!GameData.isPlayerAlive)
        {
            // 如果死了，直接 F
            grade = "DEAD";
            gradeColor = Color.red;
        }
        else
        {
            // 活著才算分，這裡假設滿分是救 5 隻，且時間小於 3 分鐘(180秒)
            if (GameData.savedCount >= 5 && GameData.timeSpent < 180)
            {
                grade = "S";
                gradeColor = Color.yellow; // 金色
            }
            else if (GameData.savedCount >= 3)
            {
                grade = "A";
                gradeColor = Color.green;
            }
            else if (GameData.savedCount >= 1)
            {
                grade = "B";
                gradeColor = Color.cyan;
            }
            else
            {
                grade = "C"; // 活著但沒救到人
                gradeColor = Color.white;
            }
        }

        ratingText.text = grade;
        ratingText.color = gradeColor;
    }

    // --- 按鈕功能 ---

    public void OnRestartClicked()
    {
        // 記得重置數據，雖然遊戲開始時通常會覆蓋，但保險起見
        GameData.savedCount = 0;
        GameData.timeSpent = 0;
        GameData.isPlayerAlive = true;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}