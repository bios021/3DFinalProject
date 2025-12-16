using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBean : MonoBehaviour
{
    [Header("死亡設定")]
    public float destroyDelay = 3.0f; // 倒下後多久消失
    public bool isDead = false;       // 是否已經死亡（唯讀）

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

        // 2. 停止動畫 (新增)
        // 停用 Animator 以防止它繼續控制模型姿勢或覆蓋旋轉
        var animator = GetComponent<Animator>();
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
