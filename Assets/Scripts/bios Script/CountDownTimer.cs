using UnityEngine;
using TMPro; // 記得引用 TextMeshPro 的命名空間

public class CountdownTimer : MonoBehaviour
{
    [Header("設定")]
    public float timeValue = 60; // 倒數總秒數 (例如 60 秒)
    public bool isTimerRunning = false; // 控制計時器是否開始

    [Header("UI 參考")]
    public TextMeshProUGUI timerText; // 用來顯示時間的文字組件

    void Start()
    {
        // 遊戲開始時啟動計時器
        isTimerRunning = true;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            if (timeValue > 0)
            {
                // 每一幀扣除經過的時間
                timeValue -= Time.deltaTime;
            }
            else
            {
                // 時間到，歸零並停止
                timeValue = 0;
                isTimerRunning = false;
                OnTimerEnd();
            }

            // 更新畫面顯示
            DisplayTime(timeValue);
        }
    }

    // 格式化並顯示時間的函數
    void DisplayTime(float timeToDisplay)
    {
        // 為了讓倒數看起來自然 (避免 0.9 秒時就顯示 0)，我們加 1 後無條件捨去
        // 或者你可以直接用 Mathf.CeilToInt(timeToDisplay)
        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }
        else if (timeToDisplay > 0)
        {
            timeToDisplay += 1;
        }

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // 使用 string.Format 格式化為 "分:秒" (例如 01:05)
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 當時間結束時執行的邏輯
    void OnTimerEnd()
    {
        Debug.Log("時間到！Game Over 或 進入下一關");
        // 在這裡加入你的遊戲結束邏輯
    }
}