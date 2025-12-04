using UnityEngine;
using UnityEngine.UI;

public class MicSettingsUI : MonoBehaviour
{
    [Header("連結")]
    public RhythmCombat gameScript; // 拖入掛有 RhythmCombat 的物件

    [Header("UI 元件")]
    public Image volumeBarFill;     // 綠色音量條 (Image Type 設為 Filled)
    public RectTransform thresholdLine; // 紅色門檻線
    public Slider sensitivitySlider;    // 藍色調整滑桿
    public RectTransform barContainer;  // 包住綠色條和紅線的父物件 (用來計算高度)

    void Start()
    {
        // 1. 初始化滑桿數值 = 目前遊戲內的門檻值
        if (gameScript != null && sensitivitySlider != null)
        {
            sensitivitySlider.value = gameScript.micThreshold;

            // 2. 監聽滑桿：當滑桿數值改變時，呼叫 OnSliderChange
            sensitivitySlider.onValueChanged.AddListener(OnSliderChange);
        }
    }

    void Update()
    {
        if (gameScript == null) return;

        // --- 顯示即時音量 (綠色條) ---
        // 使用 Lerp 讓跳動稍微平滑一點，視覺效果比較好
        if (volumeBarFill != null)
        {
            volumeBarFill.fillAmount = Mathf.Lerp(volumeBarFill.fillAmount, gameScript.CurrentVolume, Time.deltaTime * 10f);
        }
    }

    // 當玩家拉動滑桿時觸發
    public void OnSliderChange(float value)
    {
        // 1. 更新遊戲內的判定門檻
        if (gameScript != null)
        {
            gameScript.micThreshold = value;
        }

        // 2. 更新紅色線的高度
        UpdateThresholdLinePosition(value);
    }

    void UpdateThresholdLinePosition(float value)
    {
        if (thresholdLine == null || barContainer == null) return;

        // 計算高度：容器高度 * 門檻百分比(0~1)
        float height = barContainer.rect.height * value;

        // 設定紅線的 Y 軸位置 (假設紅線 Anchor 設在 Bottom Center)
        thresholdLine.anchoredPosition = new Vector2(0, height);
    }
}