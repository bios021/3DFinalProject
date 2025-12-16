using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 記得加這個

public class IntroManager : MonoBehaviour
{
    [Header("UI 面板")]
    public GameObject titlePanel;
    public GameObject infoPanel;

    [Header("故事設定")]
    public TypewriterEffect storyTextObj; // 這裡改用我們剛剛寫的腳本類型

    // 使用 [TextArea] 可以在 Inspector 裡有大框框好打字
    [TextArea(3, 10)]
    public string storyContent = "西元 2077 年...\n核心能源即將耗盡。\n\n任務目標：\n1. 潛入設施\n2. 取得電池\n3. 活著回來";

    [Header("場景設定")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        titlePanel.SetActive(false);
        infoPanel.SetActive(true);
        storyTextObj.StartTyping(storyContent);
    }

    public void OnStartButtonClicked()
    {
        titlePanel.SetActive(false);
        infoPanel.SetActive(true);

        // --- 這裡呼叫打字機 ---
        if (storyTextObj != null)
        {
            storyTextObj.StartTyping(storyContent);
        }
    }

    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}