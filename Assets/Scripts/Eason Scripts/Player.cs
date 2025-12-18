using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // �ޥ� UI �R�W�Ŷ� (�� Image ��)
using TMPro;          // �ޥ� TextMeshPro �R�W�Ŷ� (�� TMP_Text ��)
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("���a���A")]
    public int maxHealth = 5;
    public int currentHealth;
    public bool isInvincible = false; // �L�Ī��A

    [Header("���ˤ���")]
    public float scareDuration = 2.0f; // �Ϥ���ܮɶ�
    public Image scareImage;           // ��J UI Image (�~�H�Ϥ�)

    [Header("UI ���")]
    public TMP_Text healthText;        // �ק�G��� TMP_Text �H�䴩 TextMeshPro

    [Header("���ĳ]�w")]
    public AudioSource audioSource;    // ��J AudioSource �ե�
    public AudioClip scareSound;       // ��J�Q�~�쪺���� (�u�P)

    private FirstPersonController fpsController;
    private float scareTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        fpsController = GetComponent<FirstPersonController>();

        // �T�O�@�}�l�Ϥ��O���ê�
        if (scareImage != null)
        {
            scareImage.gameObject.SetActive(false);
        }

        // ��l�Ʀ�q���
        UpdateHealthUI();
    }

    void Update()
    {
        // �p�G���b�Q�~ (scareTimer > 0)
        if (scareTimer > 0)
        {
            scareTimer -= Time.deltaTime;

            // �ɶ���A��_����
            if (scareTimer <= 0)
            {
                RecoverFromScare();
            }
        }
    }

    // �Q�Ǫ��I�s
    public void TakeDamage(float damage, Transform attacker)
    {
        if (isInvincible) return;

        currentHealth -= 1; // ���@�w��
        Debug.Log($"Player ����! �Ѿl��q: {currentHealth}");

        // ��s��q UI
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Ĳ�o���~���A (��ܹϤ�)
            StartScare();
        }
    }

    // ���F�ۮe SendMessage ("TakeDamage", float)
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
        isInvincible = true; // ���~�����L��
        scareTimer = scareDuration;

        // ��w���a�ާ@
        if (fpsController != null)
        {
            fpsController.lockInput = true;
        }

        // ����~�H�Ϥ�
        if (scareImage != null)
        {
            scareImage.gameObject.SetActive(true);
        }

        // �������~����
        if (audioSource != null && scareSound != null)
        {
            audioSource.clip = scareSound;
            audioSource.Play();
        }
    }

    void RecoverFromScare()
    {
        // ����ާ@
        if (fpsController != null)
        {
            fpsController.lockInput = false;
        }
        
        isInvincible = false;
        
        // ���ùϤ�
        if (scareImage != null)
        {
            scareImage.gameObject.SetActive(false);
        }

        // ����� (�T�O���W�L���~�ɶ�)
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void Die()
        {
            Debug.Log("Player 死亡!");
            if (healthText != null) healthText.text = "YOU DIED";
            
            // 鎖定玩家操作
            if (fpsController != null) fpsController.lockInput = true;

            // ★ 呼叫結算流程
            StartCoroutine(GameOverSequence());
        }
    IEnumerator GameOverSequence()
    {
        // 步驟 A: 找到場景內所有標籤為 "NormalBean" 的物件並銷毀
        // 這符合你要求的「所有 NormalBean 都不在場上」
        GameObject[] allBeans = GameObject.FindGameObjectsWithTag("NormalBean");
        
        foreach (GameObject bean in allBeans)
        {
            // 你可以在這裡加個爆炸特效或是消失動畫，目前先直接銷毀
            Destroy(bean);
        }

        // 步驟 B: 停止/等待 3 秒
        // 這時候玩家已經死掉且不能動，糖豆也都消失了
        yield return new WaitForSeconds(3.0f);

        // 步驟 C: 導向 Result Scene
        // 請將 "ResultScene" 改成你真正的結算場景名稱
        SceneManager.LoadScene("Result");
    }

    // �i�s�W�j�^��\��
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"Player �^��! �ثe��q: {currentHealth}");
        UpdateHealthUI();
    }
}
