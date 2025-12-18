using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBean : MonoBehaviour
{
    [Header("基本設定")]
    public float destroyDelay = 3.0f; // 倒下後多久刪除
    public bool isDead = false;       // 是否已經死亡(供外部讀取)

    [Header("聽聲音移動設定")]
    public float moveSpeed = 2.0f;          // 移動速度
    public float hearingRange = 15.0f;      // 聽覺範圍 (超過這距離就聽不到)
    public float hearingThreshold = 0.15f;  // 音量門檻 (超過這音量才移動)
    public float stopDistance = 1.5f;       // 距離目標多近停止 (避免穿模)

    private RhythmCombat rhythmCombat;      // 用來取得音量和目標位置
    private Animator animator;              // 用來控制動畫

    // 【新增】被拖拽的相關變數
    private Transform dragTarget; // 拖拽目標 (通常是玩家)
    private bool isBeingDragged = false;
    private float dragStopDistance = 2.0f; // 跟玩家多近會停下來

    void Start()
    {
        // 尋找場景中的 RhythmCombat
        rhythmCombat = FindObjectOfType<RhythmCombat>();
        if (rhythmCombat == null)
        {
            Debug.LogWarning($"{name}: 找不到 RhythmCombat,無法聽聲音。");
        }

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;

        // 【新增】如果正在被拖拽,只處理拖拽邏輯
        if (isBeingDragged && dragTarget != null)
        {
            HandleDragMovement();
        }
        else
        {
            // 否則才執行本來的聽覺邏輯
            HandleSoundMovement();
        }
    }

    void HandleSoundMovement()
    {
        bool isMoving = false;

        if (rhythmCombat != null)
        {
            // 1. 取得目標位置 (玩家/輸入點位置)
            // RhythmCombat.spawnPoint 通常是玩家位置或音符生成處
            Transform target = (rhythmCombat.spawnPoint != null) ? rhythmCombat.spawnPoint : rhythmCombat.transform;

            // 2. 計算距離
            float distance = Vector3.Distance(transform.position, target.position);

            // 3. 判斷是否在聽覺範圍 且 不會撞到玩家身上
            if (distance <= hearingRange && distance > stopDistance)
            {
                // 4. 判斷音量是否夠大
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
            // 請確保 Animator Controller 有 "isWalking" (bool) 參數,並自行修改參數名稱
            animator.SetBool("isWalking", isMoving);
        }
    }

    void MoveTowards(Vector3 targetPos)
    {
        // 計算方向 (鎖定 Y 軸在水平系)
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

    // 【新增】處理被拖拽的移動
    void HandleDragMovement()
    {
        float dist = Vector3.Distance(transform.position, dragTarget.position);
        
        // 如果距離大於停止距離,就跟著目標移動
        if (dist > dragStopDistance)
        {
            MoveTowards(dragTarget.position);
            
            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            // 夠接近了,停下來
            if (animator != null) animator.SetBool("isWalking", false);
        }
    }

    // 【新增】開始被拖拽
    public void StartDragging(Transform target)
    {
        dragTarget = target;
        isBeingDragged = true;
        Debug.Log($"{name} 開始被拖拽!");
    }

    // 【新增】停止被拖拽
    public void StopDragging()
    {
        isBeingDragged = false;
        dragTarget = null;
        Debug.Log($"{name} 停止被拖拽!");
    }

    // 外部呼叫這個方法來觸發死亡流程
    public void Die()
    {
        if (isDead) return; // 避免重複觸發
        isDead = true;

        Debug.Log($"{name} 被吃了,倒下!");

        // 1. 停止行為腳本
        // 如果有 CrazyFan 之類也會攻擊玩家的腳本,就停用它
        var fanScript = GetComponent<CrazyFan>();
        if (fanScript != null) fanScript.enabled = false;

        // 如果有 NavMeshAgent,也停用它
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // 2. 停止動畫
        // 保留 Animator 以免之後控制倒下的動畫或許會用到
        if (animator != null)
        {
            animator.enabled = false;
        }

        // 3. 停用 Collider (避免屍體還能被碰撞或攻擊)
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 4. 播放倒下動畫
        StartCoroutine(FallDownRoutine());

        // 5. 幾秒後刪除
        Destroy(gameObject, destroyDelay);
    }

    // 簡單的倒下效果:在 0.5 秒內旋轉 90 度倒下
    private IEnumerator FallDownRoutine()
    {
        Quaternion startRot = transform.rotation;
        // 假設角色正向前,通常沿 X 軸或 Z 軸轉 90 度會倒下
        // 這裡假設沿 X 軸倒下 (向前倒向後)
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