using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBean : MonoBehaviour
{
    public enum State { Idle, Wander, MoveToSound, Aggro }

    [Header("基本設定")]
    public string playerTag = "Player";
    public string normalBeanTag = "NormalBean"; // 請確保一般糖豆有此 Tag
    public bool debugGizmos = true;

    [Header("狀態 1: 闲逛 (Wander)")]
    public float roamRadius = 8f;           // 闲逛半徑
    public float wanderSpeed = 1.5f;
    public float wanderChangeInterval = 3f; // 多久換一次閒逛目標

    [Header("狀態 2: 聽覺 (MoveToSound)")]
    public float hearingRange = 15f;        // 聽覺範圍
    public float hearingThreshold = 0.2f;   // 音量閾值
    public float hearingCooldown = 1f;      // 冷卻時間
    public float moveToSoundSpeed = 2.5f;

    [Header("狀態 3: 追逐 (Aggro) - 優先級最高")]
    public float aggroRange = 5f;           // 偵測範圍
    public float chaseSpeed = 3.5f;
    public float attackRange = 1.2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.0f;

    // 內部變數
    private State state = State.Wander;
    private Vector3 initialPosition;
    private Vector3 wanderTarget;
    private float lastWanderTime;
    private float lastHeardTime;
    private float lastAttackTime;

    // 參考
    private RhythmCombat rhythmCombat;
    private Transform currentTarget; // 目前追逐的目標

    void Start()
    {
        initialPosition = transform.position;
        PickNewWanderTarget();

        rhythmCombat = FindObjectOfType<RhythmCombat>();
        if (rhythmCombat == null)
        {
            Debug.LogWarning($"MonsterBean ({name}): 找不到 RhythmCombat，聽覺功能將失效。");
        }
    }

    void Update()
    {
        // --- 決策邏輯 ---

        // 1. 檢查 Aggro (視覺/感知)
        Transform aggroTarget = FindAggroTarget();
        if (aggroTarget != null)
        {
            state = State.Aggro;
            currentTarget = aggroTarget;
        }
        else
        {
            // 沒發現目標，檢查聽覺 (僅在非 Aggro 時)
            if (state != State.Aggro)
            {
                CheckHearing();
            }

            // 如果現在是 Aggro 但目標丟失了，回到 Wander
            if (state == State.Aggro)
            {
                state = State.Wander;
                currentTarget = null;
            }
        }

        // 2. 執行狀態行為
        switch (state)
        {
            case State.Wander:
                DoWander();
                break;
            case State.MoveToSound:
                DoMoveToSound();
                break;
            case State.Aggro:
                DoAggro();
                break;
        }
    }

    // --- 核心邏輯方法 ---

    private Transform FindAggroTarget()
    {
        // 1. 先找範圍內的 "活著的" NormalBean (優先級高)
        Transform nearestBean = FindNearestLiveBeanInRange(aggroRange);
        if (nearestBean != null) return nearestBean;

        // 2. 再找範圍內的 Player
        Transform nearestPlayer = FindNearestTagInRange(playerTag, aggroRange);
        if (nearestPlayer != null) return nearestPlayer;

        return null;
    }

    private void CheckHearing()
    {
        if (rhythmCombat == null) return;
        if (Time.time - lastHeardTime < hearingCooldown) return;
        if (rhythmCombat.CurrentVolume < hearingThreshold) return;

        Transform soundSource = (rhythmCombat.spawnPoint != null) ? rhythmCombat.spawnPoint : rhythmCombat.transform;
        
        float dist = Vector3.Distance(transform.position, soundSource.position);
        if (dist <= hearingRange)
        {
            lastHeardTime = Time.time;
            state = State.MoveToSound;
            wanderTarget = soundSource.position;
        }
    }

    // --- 行為實作 ---

    private void DoWander()
    {
        if (Time.time - lastWanderTime > wanderChangeInterval || Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            PickNewWanderTarget();
            lastWanderTime = Time.time;
        }
        MoveTowards(wanderTarget, wanderSpeed);
    }

    private void DoMoveToSound()
    {
        if (Vector3.Distance(transform.position, wanderTarget) < 0.8f)
        {
            state = State.Wander;
            PickNewWanderTarget();
            return;
        }
        
        if (Time.time - lastHeardTime > 5f)
        {
            state = State.Wander;
            return;
        }

        MoveTowards(wanderTarget, moveToSoundSpeed);
    }

    private void DoAggro()
    {
        if (currentTarget == null)
        {
            state = State.Wander;
            return;
        }

        // 檢查目標是否已經死亡 (如果是 NormalBean)
        NormalBean bean = currentTarget.GetComponent<NormalBean>();
        if (bean != null && bean.isDead)
        {
            currentTarget = null;
            state = State.Wander;
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        if (dist > aggroRange * 1.5f)
        {
            currentTarget = null;
            state = State.Wander;
            return;
        }

        MoveTowards(currentTarget.position, chaseSpeed);

        if (dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            DoAttack(currentTarget.gameObject);
        }
    }
    
    private void DoAttack(GameObject target)
    {
        if (target.CompareTag(normalBeanTag))
        {
            Debug.Log($"MonsterBean: 捕食一般糖豆 {target.name}");
            
            // 嘗試取得 NormalBean 腳本並呼叫 Die
            NormalBean bean = target.GetComponent<NormalBean>();
            if (bean != null)
            {
                bean.Die();
            }
            else
            {
                // 如果沒掛腳本，直接刪除 (Fallback)
                Destroy(target);
            }
            
            currentTarget = null; // 攻击后重置目标，寻找下一個
        }
        else if (target.CompareTag(playerTag))
        {
            Debug.Log($"MonsterBean: 攻擊玩家 {target.name}，傷害 {attackDamage}");
            target.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    // --- 輔助方法 ---

    private void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void PickNewWanderTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * roamRadius;
        wanderTarget = initialPosition + new Vector3(rnd.x, 0, rnd.y);
    }

    // 尋找最近且 "活著" 的 NormalBean
    private Transform FindNearestLiveBeanInRange(float range)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(normalBeanTag);
        Transform nearest = null;
        float minDist = range;

        foreach (var obj in objects)
        {
            // 檢查是否已死亡
            NormalBean bean = obj.GetComponent<NormalBean>();
            if (bean != null && bean.isDead) continue;

            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d <= minDist)
            {
                minDist = d;
                nearest = obj.transform;
            }
        }
        return nearest;
    }

    private Transform FindNearestTagInRange(string tag, float range)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        Transform nearest = null;
        float minDist = range;

        foreach (var obj in objects)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d <= minDist)
            {
                minDist = d;
                nearest = obj.transform;
            }
        }
        return nearest;
    }

    void OnDrawGizmosSelected()
    {
        if (!debugGizmos) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? initialPosition : transform.position, roamRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
