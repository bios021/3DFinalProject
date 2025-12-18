using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 引用 TextMeshPro

public class LogUIManager : MonoBehaviour
{
    [Header("UI 元件")]
    public GameObject logPanel;   // 拖入你的閱讀介面 Panel
    public TMP_Text contentText;  // 拖入顯示內容的 TextMeshPro

    private bool isReading = false;

    void Start()
    {
        // 遊戲開始時隱藏閱讀介面
        if (logPanel != null) logPanel.SetActive(false);
    }

    void Update()
    {
        // 如果正在閱讀，偵測 ESC 來關閉
        if (isReading)
        {
            // 修改這裡：只保留 ESC，移除 || Input.GetKeyDown(KeyCode.E)
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                CloseLog();
            }
        }
    }

    public void ShowLog(string content)
    {
        if (logPanel == null) return;

        // 設定文字
        if (contentText != null) contentText.text = content;
        
        // 開啟介面
        logPanel.SetActive(true);
        isReading = true;

        // (選項) 暫停遊戲時間，看你需求
        // Time.timeScale = 0f;
        
        // 解鎖滑鼠 (方便閱讀或點擊)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseLog()
    {
        if (logPanel == null) return;

        // 關閉介面
        logPanel.SetActive(false);
        isReading = false;

        // (選項) 恢復遊戲時間
        // Time.timeScale = 1f;

        // 鎖定滑鼠 (回到 FPS 模式)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsReading()
    {
        return isReading;
    }
}
