using UnityEngine;
using UnityEngine.UI;

public class RhythmCombat : MonoBehaviour
{
    [Header("節奏設定")]
    public float bpm = 120f;
    public float timeWindow = 0.15f;

    [Header("UI 設定 (滑動式軌道)")]
    public RectTransform leftBar;       // 左邊移動的短線
    public RectTransform rightBar;      // 右邊移動的短線
    public Image centerMarker;          // 中間不動的基準線 (用來變色回饋)

    public float trackWidth = 200f;     // 軌道總寬度的一半 (也就是 Bar 開始的距離)

    public Color normalColor = Color.white;   // 平常顏色
    public Color successColor = Color.cyan;   // 成功時的顏色 (建議亮一點)

    [Header("麥克風設定")]
    [Range(0.001f, 1f)]
    public float micThreshold = 0.1f;
    public float inputCooldown = 0.3f;
    public bool showDebugLog = true;

    [Header("視覺與音效")]
    public GameObject shockwavePrefab;
    public Transform spawnPoint;
    public AudioSource beatMusic;
    public AudioSource sfxSource;
    public AudioClip successClip;

    [Range(1f, 100f)]
    public float sensitivityMultiplier = 1000f; // 預設放大 10 倍

    [Header("震波微調")]
    public Vector3 effectRotation = new Vector3(90, 0, 0);
    public float heightOffset = 0.1f;

    // --- 內部變數 ---
    private float beatInterval;
    private float timer;

    // 麥克風變數
    private AudioClip micClip;
    private string micName;
    private float lastTriggerTime;
    private int sampleWindow = 128;

    void Start()
    {
        beatInterval = 60f / bpm;
        if (beatMusic != null) beatMusic.Play();

        // 初始化顏色
        if (centerMarker != null) centerMarker.color = normalColor;

        // 初始化麥克風
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            micClip = Microphone.Start(micName, true, 1, 44100);
        }
    }

    public float CurrentVolume { get; private set; } // 讓外部只能讀取，不能修改

    // 修改 Update 函式
    void Update()
    {
        timer += Time.deltaTime;
        UpdateSliderUI();

        // 取得音量
        float currentLoudness = GetLoudnessFromMic();

        // --- 新增這一行：將音量存到公開變數 ---
        CurrentVolume = currentLoudness;
        // ------------------------------------

        if (showDebugLog && currentLoudness > 0.01f)
            Debug.Log($"目前音量: {currentLoudness}");

        // 原本的判斷邏輯保持不變...
        if (currentLoudness > micThreshold && (Time.time - lastTriggerTime) > inputCooldown)
        {
            lastTriggerTime = Time.time;
            CheckRhythm();
        }
    }

    // --- 新增：兩側往中間移動的動畫 ---
    void UpdateSliderUI()
    {
        if (leftBar == null || rightBar == null) return;

        // 算出目前進度 0.0 ~ 1.0
        // 0.0 = 節拍剛開始 (最遠)
        // 1.0 = 節拍點 (中間)
        float progress = (timer % beatInterval) / beatInterval;

        // 計算現在應該在的 X 軸位置
        // Mathf.Lerp(開始位置, 結束位置, 進度)
        // 我們希望從 trackWidth 移動到 0
        float currentX = Mathf.Lerp(trackWidth, 0f, progress);

        // 設定左邊 Bar 的位置 (X 為負值)
        leftBar.anchoredPosition = new Vector2(-currentX, 0);

        // 設定右邊 Bar 的位置 (X 為正值)
        rightBar.anchoredPosition = new Vector2(currentX, 0);

        // 顏色慢慢淡出回正常色 (如果剛剛成功變色了)
        if (centerMarker != null)
        {
            centerMarker.color = Color.Lerp(centerMarker.color, normalColor, Time.deltaTime * 5f);
        }
    }

    // ... (GetLoudnessFromMic 跟原本一樣，省略以節省篇幅) ...
    float GetLoudnessFromMic()
    {
        if (micClip == null) return 0;
        int micPosition = Microphone.GetPosition(micName);
        int startPosition = micPosition - sampleWindow;
        if (startPosition < 0) return 0;
        float[] waveData = new float[sampleWindow];
        micClip.GetData(waveData, startPosition);
        float totalLoudness = 0;
        for (int i = 0; i < sampleWindow; i++) totalLoudness += Mathf.Abs(waveData[i]);
        float average = totalLoudness / sampleWindow;
        return Mathf.Clamp01(average * sensitivityMultiplier);
    }

    void CheckRhythm()
    {
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
            Debug.Log("Miss!");
            // 可選：失敗時中間變紅色
            // if (centerMarker != null) centerMarker.color = Color.red;
        }
    }

    void OnPerfectHit()
    {
        Debug.Log("Perfect!");

        // UI 回饋：中間那根線變亮色
        if (centerMarker != null)
        {
            centerMarker.color = successColor;
        }

        // 生成特效與音效
        if (shockwavePrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            pos.y += heightOffset;
            Instantiate(shockwavePrefab, pos, Quaternion.Euler(effectRotation));
        }

        if (sfxSource != null && successClip != null)
        {
            sfxSource.PlayOneShot(successClip);
        }
    }
}