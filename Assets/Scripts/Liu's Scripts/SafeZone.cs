using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafeZone : MonoBehaviour
{
    [Header("UI Settings")]
    // 雖然有兩個 SafeZone，但請把同一個 UI Text 物件拖進這兩個 SafeZone 的欄位裡
    public TMP_Text scoreText; 
    
    [Header("Audio Settings")]
    public AudioClip saveSound; 
    private AudioSource audioSource;
    
    [Header("Effect Settings")]
    public GameObject saveEffectPrefab; 
    
    // 【重點修改 1】加上 static，讓所有 SafeZone 共用這個變數
    private static int globalSavedCount = 0;
    
    // 【重點修改 2】HashSet 也變成 static，防止糖豆在 A區救過後，跑到 B區又被算一次
    private static HashSet<GameObject> globalSavedBeans = new HashSet<GameObject>(); 

    // 【新增】這個方法保證每次遊戲開始(按 Play 時)分數都會歸零，不然 static 變數會一直累積
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticVariables()
    {
        globalSavedCount = 0;
        globalSavedBeans.Clear();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && saveSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 確保剛開始遊戲時 UI 是正確的
        UpdateScoreUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NormalBean"))
        {
            // 檢查是否在「任何一個」SafeZone 被救過了
            if (globalSavedBeans.Contains(other.gameObject))
            {
                // 這裡註解掉 Debug，避免多個 SafeZone 重疊時一直跳訊息
                // Debug.Log($"{other.name} already saved."); 
                return;
            }
            
            NormalBean bean = other.GetComponent<NormalBean>();
            if (bean != null && bean.isDead)
            {
                return;
            }
            
            SaveBean(other.gameObject);
        }
    }

    private void SaveBean(GameObject bean)
    {
        // 加入全域清單
        globalSavedBeans.Add(bean);
        globalSavedCount++; // 增加全域分數
        
        // 更新 UI (因為是 static 分數，所以不管誰呼叫這行，顯示的總分都一樣)
        UpdateScoreUI();
        
        if (audioSource != null && saveSound != null)
        {
            audioSource.PlayOneShot(saveSound);
        }
        
        if (saveEffectPrefab != null)
        {
            Instantiate(saveEffectPrefab, bean.transform.position, Quaternion.identity);
        }
        
        Debug.Log($"Saved {bean.name}! Global Total: {globalSavedCount}");
        
        StartCoroutine(RemoveBeanAfterDelay(bean, 0.5f));
    }

    private IEnumerator RemoveBeanAfterDelay(GameObject bean, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(bean != null) Destroy(bean);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Saved: {globalSavedCount}"; 
        }
    }

    // 供外部取得總分
    public int GetSavedBeanCount()
    {
        return globalSavedCount;
    }

    // 手動重置分數
    public void ResetScore()
    {
        ResetStaticVariables();
        UpdateScoreUI();
    }
}