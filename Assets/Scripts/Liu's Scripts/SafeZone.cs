using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafeZone : MonoBehaviour
{
    [Header("UI Settings")]
    public TMP_Text scoreText; // 拖入 Canvas 上的 TextMeshPro 組件
    
    [Header("Audio Settings (Optional)")]
    public AudioClip saveSound; 
    private AudioSource audioSource;
    
    [Header("Effect Settings (Optional)")]
    public GameObject saveEffectPrefab; 
    
    // 內部變數
    private int savedBeanCount = 0;
    private HashSet<GameObject> savedBeans = new HashSet<GameObject>(); 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && saveSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        UpdateScoreUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NormalBean"))
        {
            // 檢查是否重複
            if (savedBeans.Contains(other.gameObject))
            {
                Debug.Log($"{other.name} already saved."); // 改成英文
                return;
            }
            
            // 檢查是否死亡
            NormalBean bean = other.GetComponent<NormalBean>();
            if (bean != null && bean.isDead)
            {
                Debug.Log($"{other.name} is dead, not counting."); // 改成英文
                return;
            }
            
            SaveBean(other.gameObject);
        }
    }

    private void SaveBean(GameObject bean)
    {
        savedBeans.Add(bean);
        savedBeanCount++;
        
        UpdateScoreUI();
        
        if (audioSource != null && saveSound != null)
        {
            audioSource.PlayOneShot(saveSound);
        }
        
        if (saveEffectPrefab != null)
        {
            Instantiate(saveEffectPrefab, bean.transform.position, Quaternion.identity);
        }
        
        // Log 改成英文
        Debug.Log($"Saved {bean.name}! Total: {savedBeanCount}");
        
        StartCoroutine(RemoveBeanAfterDelay(bean, 0.5f));
    }

    private IEnumerator RemoveBeanAfterDelay(GameObject bean, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bean);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // 【重點修改】這裡改成英文，就不會變方塊亂碼了
            scoreText.text = $"Saved: {savedBeanCount}"; 
        }
    }

    public int GetSavedBeanCount()
    {
        return savedBeanCount;
    }

    public void ResetScore()
    {
        savedBeanCount = 0;
        savedBeans.Clear();
        UpdateScoreUI();
    }
}