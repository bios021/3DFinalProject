using UnityEngine;
using TMPro; // 記得引用
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [Header("設定")]
    public float typingSpeed = 0.05f; // 打字速度 (越小越快)

    private TextMeshProUGUI textComponent;
    private string fullText; // 暫存完整的文字內容
    private Coroutine typingCoroutine; // 用來記錄正在跑的協程

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    // 給外部 (IntroManager) 呼叫的函式
    public void StartTyping(string textToType)
    {
        fullText = textToType;
        textComponent.text = ""; // 先清空文字

        // 如果原本有在打字，先停掉，避免重疊
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    // 這是核心 Coroutine
    IEnumerator TypeText()
    {
        foreach (char letter in fullText)
        {
            textComponent.text += letter; // 一個字一個字加上去
            yield return new WaitForSeconds(typingSpeed); // 等待一小段時間
        }
        typingCoroutine = null; // 打完了
    }

    // (選用) 讓玩家點擊時可以瞬間顯示全部
    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            textComponent.text = fullText; // 直接顯示全部
            typingCoroutine = null;
        }
    }
}