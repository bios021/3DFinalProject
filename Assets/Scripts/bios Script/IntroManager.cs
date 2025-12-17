using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class IntroManager : MonoBehaviour
{
    [Header("=== UI 面板設定 ===")]
    public GameObject titlePanel;
    public GameObject infoPanel;
    public GameObject blackBackground;
    public TypewriterEffect storyTextObj;

    // ★ 新增：提示玩家發出聲音的文字 (選填)
    public TextMeshProUGUI micHintText; // 例如顯示：「請大叫一聲來開啟大門！」

    [Header("=== 故事設定 ===")]
    [TextArea(3, 10)]
    public string storyContent = "糖豆天生看不見...";

    [Header("=== 麥克風設定 (新功能) ===")]
    [Range(0f, 1f)]
    public float micThreshold = 0.3f; // 觸發開門的音量門檻 (0~1)，越小越靈敏
    public bool showDebugVolume = true; // 是否在 Console 顯示目前音量 (測試用)

    [Header("=== 門的動畫設定 ===")]
    public Renderer[] allRenderersToFade;
    public Transform doorPivot;
    public float doorFadeDuration = 2.0f;
    public float doorOpenDuration = 1.5f;
    public float doorOpenAngle = 90f;

    [Header("=== 場景設定 ===")]
    public string gameSceneName = "GameScene";

    // 內部變數
    private AudioClip micClip; // 用來儲存麥克風錄到的聲音
    private string deviceName; // 麥克風名稱
    private List<Material> allMaterials = new List<Material>();
    private List<Color> startColors = new List<Color>();
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    void Start()
    {
        // 1. 初始化介面
        titlePanel.SetActive(false);
        infoPanel.SetActive(true);
        if (micHintText != null) micHintText.gameObject.SetActive(false); // 先隱藏提示

        // 2. 初始化門材質
        foreach (Renderer r in allRenderersToFade)
        {
            if (r != null)
            {
                Material mat = r.material;
                allMaterials.Add(mat);
                startColors.Add(mat.color);
                Color transparentColor = mat.color;
                transparentColor.a = 0f;
                mat.color = transparentColor;
            }
        }

        // 3. 初始化門角度
        if (doorPivot != null)
        {
            initialRotation = doorPivot.rotation;
            targetRotation = initialRotation * Quaternion.Euler(0, doorOpenAngle, 0);
        }

        // 4. 開始流程
        StartCoroutine(AutoPlayStorySequence());
    }

    IEnumerator AutoPlayStorySequence()
    {
        // === 階段一：打字 ===
        if (storyTextObj != null)
        {
            storyTextObj.StartTyping(storyContent);
            float typingDuration = storyContent.Length * storyTextObj.typingSpeed;
            yield return new WaitForSeconds(typingDuration+3f);
        }

        // === 階段二：啟動麥克風並等待玩家發聲 ===
        Debug.Log("文字跑完，開啟麥克風，等待玩家聲音...");

        // 顯示提示文字 (如果有設定)
        if (micHintText != null)
        {
            micHintText.text = "（請對著麥克風發出聲音...）";
            micHintText.gameObject.SetActive(true);
        }

        

        // === 階段三：關閉 UI ===
        Debug.Log("準備開門");
        infoPanel.SetActive(false); // 關閉文字框
        if (blackBackground != null) blackBackground.SetActive(false); // 關閉黑布

        // === 階段四：門浮現 ===
        if (allMaterials.Count > 0) yield return StartCoroutine(FadeInDoorGroup());

        // 啟動麥克風
        StartMicrophone();

        // ★ 死循環：卡在這裡直到音量超標
        bool soundDetected = false;
        while (!soundDetected)
        {
            float currentVol = GetMicVolume();

            if (showDebugVolume) Debug.Log("目前音量: " + currentVol); // 測試用，看數值來調整 Threshold

            if (currentVol > micThreshold)
            {
                soundDetected = true;
                Debug.Log("偵測到足夠音量！開門！");
            }
            yield return null; // 等待下一幀繼續偵測
        }

        // 停止麥克風
        StopMicrophone();

        // === 階段五：門打開 ===
        if (doorPivot != null) yield return StartCoroutine(OpenDoorRotation());

        // === 階段六：進遊戲 ===
        yield return new WaitForSeconds(1f);
        OnPlayButtonClicked();
    }

    // --- 麥克風相關功能 ---
    void StartMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0]; // 抓第一個麥克風
            // 錄製長度10秒(會循環)，頻率44100
            micClip = Microphone.Start(deviceName, true, 10, 44100);
        }
        else
        {
            Debug.LogError("找不到麥克風！直接略過測試。");
            // 如果沒麥克風，為了避免卡死，這裡直接讓它繼續
        }
    }

    void StopMicrophone()
    {
        if (Microphone.IsRecording(deviceName))
        {
            Microphone.End(deviceName);
        }
    }

    // 計算目前的音量 (0~1)
    float GetMicVolume()
    {
        if (micClip == null) return 1f; // 如果沒麥克風，回傳最大值讓它直接通過

        int sampleSize = 128;
        float[] samples = new float[sampleSize];
        int startPosition = Microphone.GetPosition(deviceName) - (sampleSize + 1);

        if (startPosition < 0) return 0;

        // 抓取最近一段的聲音數據
        micClip.GetData(samples, startPosition);

        // 找出這段數據中的最大音量 (Peak)
        float maxVolume = 0f;
        foreach (var sample in samples)
        {
            float waveHeight = Mathf.Abs(sample);
            if (waveHeight > maxVolume) maxVolume = waveHeight;
        }
        return maxVolume;
    }

    // --- 以下為原本的門動畫與按鈕程式碼 (保持不變) ---
    public void OnStartButtonClicked() { /*...*/ }
    public void OnPlayButtonClicked() { SceneManager.LoadScene(gameSceneName); }

    IEnumerator FadeInDoorGroup()
    {
        float timer = 0f;
        while (timer < doorFadeDuration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, timer / doorFadeDuration);
            for (int i = 0; i < allMaterials.Count; i++)
            {
                Color currentColor = startColors[i];
                currentColor.a = newAlpha;
                allMaterials[i].color = currentColor;
            }
            yield return null;
        }
        for (int i = 0; i < allMaterials.Count; i++)
        {
            Color finalColor = startColors[i];
            finalColor.a = 1f;
            allMaterials[i].color = finalColor;
        }
    }

    IEnumerator OpenDoorRotation()
    {
        float timer = 0f;
        while (timer < doorOpenDuration)
        {
            timer += Time.deltaTime;
            doorPivot.rotation = Quaternion.Slerp(initialRotation, targetRotation, timer / doorOpenDuration);
            yield return null;
        }
        doorPivot.rotation = targetRotation;
    }
}