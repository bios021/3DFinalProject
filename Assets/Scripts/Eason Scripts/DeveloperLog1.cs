using UnityEngine;

public class DeveloperLog1 : MonoBehaviour
{
    [Header("日誌內容")]
    [TextArea(5, 10)] // 讓輸入框變大，方便打多行文字
    public string logContent = "實驗日誌 #402\n\n受試者反應劇烈，細胞增生速度超出預期...\n我們必須隔離實驗室。";
}
