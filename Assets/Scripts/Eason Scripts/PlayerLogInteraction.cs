using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerLogInteraction : MonoBehaviour
{
    [Header("偵測設定")]
    public float interactDistance = 3.0f; // 互動距離
    public LayerMask interactLayer;       // 建議設定 Layer (例如 Default) 避免誤判

    [Header("UI 提示")]
    public TMP_Text hintText; // 螢幕準心附近的提示文字 (例如: 按 E 閱讀)

    private Camera playerCam;
    private LogUIManager uiManager;

    void Start()
    {
        playerCam = Camera.main; // 抓取主攝影機
        uiManager = FindObjectOfType<LogUIManager>(); // 自動尋找場景中的 UI 管理器
    }

    void Update()
    {
        // 如果正在讀日誌，就不執行偵測，並清空提示
        if (uiManager != null && uiManager.IsReading())
        {
            if (hintText != null) hintText.text = "";
            return;
        }

        DetectLog();
    }

    void DetectLog()
    {
        // 從攝影機位置向前方發射射線
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit hit;

        // 發射射線
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // 檢查打到的物件是否有 DeveloperLog 腳本
            DeveloperLog1 log = hit.collider.GetComponent<DeveloperLog1>();
            if (log != null)
            {
                // 1. 顯示提示
                if (hintText != null) hintText.text = "press [E] to read diary";

                // 2. 偵測輸入
                if (Input.GetKeyDown(KeyCode.E))
                {
                    uiManager.ShowLog(log.logContent);
                }
                return; // 找到目標後就結束，避免執行下方的清空程式碼
            }
        }

        // 如果沒打到任何東西，或打到的不是日誌，清空提示
        if (hintText != null) hintText.text = "";
    }
}
