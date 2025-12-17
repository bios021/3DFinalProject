using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBean : MonoBehaviour
{
    [Header("死亡設定")]
    public float destroyDelay = 3.0f; // 倒下後多久消失
    public bool isDead = false;       // 是否已經死亡（唯讀）

    [Header("聲音引導移動設定")]
    public float moveSpeed = 2.0f;          // 移動速度
    public float hearingRange = 15.0f;      // 聽覺範圍 (超過此距離聽不到)
    public float hearingThreshold = 0.15f;  // 音量閾值 (超過此音量才移動)
    public float stopDistance = 1.5f;       // 距離玩家多近停止 (避免穿模)

    private RhythmCombat rhythmCombat;      // 用來取得音量與玩家位置
    private Animator animator;              // 用來控制動畫

    void Start()
    {
        // 取得場景中的 RhythmCombat
        rhythmCombat = FindObjectOfType<RhythmCombat>();
        if (rhythmCombat == null)
        {
            Debug.LogWarning($"{name}: 找不到 RhythmCombat，無法偵測聲音。");
        }

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return; // 死亡後不執行移動邏輯

        HandleSoundMovement();
    }

    void HandleSoundMovement()
    {
        bool isMoving = false;

        if (rhythmCombat != null)
        {
            // 1. 取得目標位置 (玩家/麥克風位置)
            // RhythmCombat.spawnPoint 通常是玩家位置或聲源中心
            Transform target = (rhythmCombat.spawnPoint != null) ? rhythmCombat.spawnPoint : rhythmCombat.transform;

            // 2. 計算距離
            float distance = Vector3.Distance(transform.position, target.position);

            // 3. 判斷是否在聽覺範圍內 且 尚未貼到玩家臉上
            if (distance <= hearingRange && distance > stopDistance)
            {
                // 4. 判斷音量是否足夠
                if (rhythmCombat.CurrentVolume > hearingThreshold)
                {
                    MoveTowards(target.position);
                    isMoving = true;
                }
            }
        }

        // 5. 更新動畫 (若有 Animator)
        if (animator != null)
        {
            // 請確保 Animator Controller 有 "isWalking" (bool) 參數，或自行修改參數名稱
            animator.SetBool("isWalking", isMoving);
        }
    }

    void MoveTowards(Vector3 targetPos)
    {
        // 計算方向 (忽略 Y 軸高度差)
        Vector3 dir = targetPos - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            // 轉向目標
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

            // 前進
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }

    // 呼叫此函式來觸發死亡流程
    public void Die()
    {
        if (isDead) return; // 避免重複觸發
        isDead = true;

        Debug.Log($"{name} 被抓到了，倒下！");

        // 1. 停止移動行為
        // 如果有 CrazyFan 腳本（會衝向舞台），將其關閉
        var fanScript = GetComponent<CrazyFan>();
        if (fanScript != null) fanScript.enabled = false;

        // 如果有 NavMeshAgent，也應該關閉
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // 2. 停止動畫
        // 停用 Animator 以防止它繼續控制模型姿勢或覆蓋旋轉
        if (animator != null)
        {
            animator.enabled = false;
        }

        // 3. 移除 Collider (避免屍體擋路或被重複偵測)
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 4. 執行倒下動畫
        StartCoroutine(FallDownRoutine());

        // 5. 倒數銷毀
        Destroy(gameObject, destroyDelay);
    }

    // 簡單的倒下效果：在 0.5 秒內旋轉 90 度躺平
    private IEnumerator FallDownRoutine()
    {
        Quaternion startRot = transform.rotation;
        // 視模型軸向而定，通常繞 X 軸或 Z 軸旋轉 90 度會倒下
        // 這裡假設繞 X 軸倒下 (向前或向後)
        Quaternion endRot = startRot * Quaternion.Euler(-90, 0, 0); 
        
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;
    }
}
