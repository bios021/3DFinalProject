using UnityEngine;

public class RhythmCombat : MonoBehaviour
{
    [Header("節奏設定")]
    public float bpm = 120f;
    public float timeWindow = 0.15f;

    [Header("麥克風設定 (新功能)")]
    [Range(0.001f, 1f)]
    public float micThreshold = 0.1f;    // 音量超過多少算觸發？(需根據麥克風靈敏度調整)
    public float inputCooldown = 0.3f;   // 觸發後多久才能再次觸發 (防止一聲吼叫觸發多次)
    public bool showDebugLog = true;     // 是否在 Console 顯示目前音量 (方便除錯)

    [Header("視覺與音效")]
    public GameObject shockwavePrefab;
    public Transform spawnPoint;
    public AudioSource beatMusic;
    public AudioSource sfxSource;
    public AudioClip successClip;

    [Header("震波微調")]
    public Vector3 effectRotation = new Vector3(90, 0, 0); // 預設 X=90 讓它躺平
    public float heightOffset = 0.1f; // 離地高度，防止跟地板重疊閃爍

    // --- 內部變數 ---
    private float beatInterval;
    private float timer;

    // 麥克風相關變數
    private AudioClip micClip;          // 儲存麥克風錄製的數據
    private string micName;             // 麥克風設備名稱
    private float lastTriggerTime;      // 上次觸發的時間
    private int sampleWindow = 128;     // 取樣窗口大小 (用來分析音量)

    void Start()
    {
        beatInterval = 60f / bpm;
        if (beatMusic != null) beatMusic.Play();

        // --- 1. 初始化麥克風 ---
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0]; // 使用預設麥克風
            // 開始錄音：(設備名, 是否循環, 長度秒數, 頻率)
            // 不需要很長，1秒循環即可，我們只取當下的音量
            micClip = Microphone.Start(micName, true, 1, 44100);
            Debug.Log("麥克風已啟動: " + micName);
        }
        else
        {
            Debug.LogError("找不到麥克風！請檢查設備。");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // --- 2. 取得麥克風音量並判斷觸發 ---
        float currentLoudness = GetLoudnessFromMic();

        // 如果想看目前的音量數值來調整 Threshold，可以看 Console
        if (showDebugLog && currentLoudness > 0.01f)
            Debug.Log($"目前音量: {currentLoudness}");

        // 條件：音量大於門檻值 && 已經過了冷卻時間
        if (currentLoudness > micThreshold && (Time.time - lastTriggerTime) > inputCooldown)
        {
            lastTriggerTime = Time.time; // 更新觸發時間
            CheckRhythm();               // 執行原本的節奏判定
        }
    }

    // --- 新增：計算麥克風響度 (RMS算法) ---
    float GetLoudnessFromMic()
    {
        if (micClip == null) return 0;

        // 取得麥克風目前的錄音位置
        int micPosition = Microphone.GetPosition(micName);

        // 為了安全，往回抓一點點資料，避免讀取邊界問題
        int startPosition = micPosition - sampleWindow;
        if (startPosition < 0) return 0; // 剛開始還沒錄到資料先回傳 0

        // 建立一個暫存陣列來放聲音數據
        float[] waveData = new float[sampleWindow];
        micClip.GetData(waveData, startPosition);

        // 計算 RMS (均方根) 振幅 -> 這比單純取最大值更能代表「響度」
        float totalLoudness = 0;
        for (int i = 0; i < sampleWindow; i++)
        {
            totalLoudness += Mathf.Abs(waveData[i]);
        }

        return totalLoudness / sampleWindow; // 回傳平均音量
    }

    void CheckRhythm()
    {
        // ... (這部分邏輯不變) ...
        float beatPosition = timer / beatInterval;
        float nearestBeat = Mathf.Round(beatPosition);
        float difference = Mathf.Abs(beatPosition - nearestBeat);
        float timeDiff = difference * beatInterval;

        if (timeDiff <= timeWindow)
        {
            OnPerfectHit();
        }
        else
        {
            Debug.Log("Miss! 聲音有出來，但節奏不對");
        }
    }

    void OnPerfectHit()
    {
        Debug.Log("Perfect! 聲波攻擊！");

        if (shockwavePrefab != null)
        {
            // 1. 設定位置：使用腳底位置，並稍微往上抬一點點 (防止與地板 Z-Fighting)
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            pos.y += heightOffset;

            // 2. 設定旋轉：使用我們在 Inspector 設定的角度
            Quaternion rotation = Quaternion.Euler(effectRotation);

            // 3. 生成
            Instantiate(shockwavePrefab, pos, rotation);
        }

        if (sfxSource != null && successClip != null)
        {
            sfxSource.PlayOneShot(successClip);
        }
    }
}