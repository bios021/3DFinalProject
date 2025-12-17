using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBean : MonoBehaviour
{
    // 新增 Intimidate (威嚇) 與 Retreat (撤退) 狀態
    public enum State { Idle, Wander, MoveToSound, Aggro, Intimidate, Retreat }

    [Header("基本設定")]
    public string playerTag = "Player";
    public string normalBeanTag = "NormalBean"; 
    public bool debugGizmos = true;

    [Header("狀態 1: 閒逛 (Wander)")]
    public float roamRadius = 8f;           
    public float wanderSpeed = 1.5f;
    public float wanderChangeInterval = 3f; 

    [Header("狀態 2: 聽覺 (MoveToSound)")]
    public float hearingRange = 15f;        
    public float hearingThreshold = 0.2f;   
    public float hearingCooldown = 1f;      
    public float moveToSoundSpeed = 2.5f;

    [Header("狀態 3: 追逐 (Aggro)")]
    public float aggroRange = 5f;           
    public float chaseSpeed = 3.5f;
    public float attackRange = 1.2f; // 攻擊距離
    public float stopDistance = 1.0f; // 停止距離 (避免穿模)
    public float attackDamage = 10f;
    public float attackCooldown = 1.0f;

    [Header("狀態 4: 威嚇與撤退 (Intimidate & Retreat)")]
    public float intimidateDuration = 2.0f; // 貼臉時間 (需配合 Player 的 scareDuration)
    public float retreatDistance = 5.0f;    // 攻擊後退多遠
    public float retreatSpeed = 2.0f;

    // 內部變數
    private State state = State.Wander;
    private Vector3 initialPosition;
    private Vector3 wanderTarget;
    private float lastWanderTime;
    private float lastHeardTime;
    private float lastAttackTime;
    private float stateTimer = 0f; // 通用計時器 (給 Intimidate 和 Retreat 用)

    // 參考
    private RhythmCombat rhythmCombat;
    private Transform currentTarget; 

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
        // 如果處於特殊狀態 (威嚇或撤退)，鎖定決策，直到狀態結束
        if (state == State.Intimidate || state == State.Retreat)
        {
            // 繼續執行當前狀態行為
        }
        else
        {
            // 一般決策 (Aggro > Sound > Wander)
            Transform aggroTarget = FindAggroTarget();
            if (aggroTarget != null)
            {
                state = State.Aggro;
                currentTarget = aggroTarget;
            }
            else
            {
                if (state != State.Aggro) CheckHearing();
                if (state == State.Aggro)
                {
                    state = State.Wander;
                    currentTarget = null;
                }
            }
        }

        // --- 執行狀態行為 ---
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
            case State.Intimidate:
                DoIntimidate();
                break;
            case State.Retreat:
                DoRetreat();
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

        // 檢查目標是否死亡 (NormalBean)
        NormalBean bean = currentTarget.GetComponent<NormalBean>();
        if (bean != null && bean.isDead)
        {
            currentTarget = null;
            state = State.Wander;
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        // 追逐距離限制
        if (dist > aggroRange * 1.5f)
        {
            currentTarget = null;
            state = State.Wander;
            return;
        }

        // 移動 (保持 stopDistance 避免穿模)
        if (dist > stopDistance)
        {
            MoveTowards(currentTarget.position, chaseSpeed);
        }
        else
        {
            // 已經很近了，面向目標
            Vector3 dir = currentTarget.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }

        // 攻擊判定
        if (dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            DoAttack(currentTarget.gameObject);
        }
    }

    private void DoIntimidate()
    {
        // 威嚇狀態：貼著玩家
        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            
            // 保持在極近距離 (例如 0.8f)，但不穿模
            float intimidateDist = 2.5f; 
            
            if (dist > intimidateDist)
            {
                MoveTowards(currentTarget.position, chaseSpeed); // 追上去貼臉
            }
            
            // 強制面向玩家
            Vector3 dir = currentTarget.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }

        // 計時結束 -> 進入撤退
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            state = State.Retreat;
            // 設定撤退目標 (背對玩家的方向)
            if (currentTarget != null)
            {
                Vector3 awayDir = (transform.position - currentTarget.position).normalized;
                wanderTarget = transform.position + awayDir * retreatDistance;
            }
            else
            {
                PickNewWanderTarget(); // 玩家不見了就隨便跑
            }
        }
    }

    private void DoRetreat()
    {
        // 撤退狀態：遠離玩家
        MoveTowards(wanderTarget, retreatSpeed);

        // 到達撤退點或距離夠遠 -> 回到 Wander
        if (Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            state = State.Wander;
            currentTarget = null; // 放棄仇恨
            lastAttackTime = Time.time; // 重置攻擊冷卻，避免立刻回頭咬
        }
    }

    private void DoAttack(GameObject target)
    {
        if (target.CompareTag(normalBeanTag))
        {
            Debug.Log($"MonsterBean: 捕食一般糖豆 {target.name}");
            NormalBean bean = target.GetComponent<NormalBean>();
            if (bean != null) bean.Die();
            else Destroy(target);
            currentTarget = null;
        }
        else if (target.CompareTag(playerTag))
        {
            Debug.Log($"MonsterBean: 攻擊玩家 {target.name}");
            
            // 呼叫 Player 的 TakeDamage，並傳入自己 (attacker)
            Player playerScript = target.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(attackDamage, transform);
                
                // 進入威嚇狀態
                state = State.Intimidate;
                stateTimer = intimidateDuration;
            }
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
            
            // 簡單防穿模：如果前方有障礙物就不移動 (或是依賴 CharacterController/Rigidbody)
            // 這裡假設用 Transform 移動，所以依賴 stopDistance 邏輯
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
