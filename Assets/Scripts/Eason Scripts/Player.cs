using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 引用 UI 命名空間 (給 Image 用)
using TMPro;          // 引用 TextMeshPro 命名空間 (給 TMP_Text 用)

public class Player : MonoBehaviour
{
    [Header("玩家狀態")]
    public int maxHealth = 5;
    public int currentHealth;
    public bool isInvincible = false; // 無敵狀態

    [Header("受傷反應")]
    public float scareDuration = 2.0f; // 圖片顯示時間
    public Image scareImage;           // 拖入 UI Image (嚇人圖片)

    [Header("UI 顯示")]
    public TMP_Text healthText;        // 修改：改用 TMP_Text 以支援 TextMeshPro

    [Header("音效設定")]
    public AudioSource audioSource;    // 拖入 AudioSource 組件
    public AudioClip scareSound;       // 拖入被嚇到的音效 (短促)

    private FirstPersonController fpsController;
    private float scareTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        fpsController = GetComponent<FirstPersonController>();

        // 確保一開始圖片是隱藏的
        if (scareImage != null)
        {
            scareImage.gameObject.SetActive(false);
        }

        // 初始化血量顯示
        UpdateHealthUI();
    }

    void Update()
    {
        // 如果正在被嚇 (scareTimer > 0)
        if (scareTimer > 0)
        {
            scareTimer -= Time.deltaTime;

            // 時間到，恢復控制
            if (scareTimer <= 0)
            {
                RecoverFromScare();
            }
        }
    }

    // 被怪物呼叫
    public void TakeDamage(float damage, Transform attacker)
    {
        if (isInvincible) return;

        currentHealth -= 1; // 扣一滴血
        Debug.Log($"Player 受傷! 剩餘血量: {currentHealth}");

        // 更新血量 UI
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 觸發驚嚇狀態 (顯示圖片)
            StartScare();
        }
    }

    // 為了相容 SendMessage ("TakeDamage", float)
    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
        currentHealth -= 1;
        
        UpdateHealthUI();

        if (currentHealth <= 0) Die();
        else StartScare();
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth} / {maxHealth}";
        }
    }

    void StartScare()
    {
        isInvincible = true; // 驚嚇期間無敵
        scareTimer = scareDuration;

        // 鎖定玩家操作
        if (fpsController != null)
        {
            fpsController.lockInput = true;
        }

        // 顯示嚇人圖片
        if (scareImage != null)
        {
            scareImage.gameObject.SetActive(true);
        }

        // 播放驚嚇音效
        if (audioSource != null && scareSound != null)
        {
            audioSource.clip = scareSound;
            audioSource.Play();
        }
    }

    void RecoverFromScare()
    {
        // 解鎖操作
        if (fpsController != null)
        {
            fpsController.lockInput = false;
        }
        
        isInvincible = false;
        
        // 隱藏圖片
        if (scareImage != null)
        {
            scareImage.gameObject.SetActive(false);
        }

        // 停止音效 (確保不超過驚嚇時間)
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void Die()
    {
        Debug.Log("Player 死亡!");
        if (healthText != null) healthText.text = "YOU DIED";
        
        // 處理死亡邏輯 (例如重置場景或顯示 Game Over)
        if (fpsController != null) fpsController.lockInput = true;
    }

    // 【新增】回血功能
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"Player 回血! 目前血量: {currentHealth}");
        UpdateHealthUI();
    }
}
