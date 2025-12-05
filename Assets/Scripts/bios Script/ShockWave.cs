using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Shockwave : MonoBehaviour
{
    [Header("波動參數")]
    public float damage = 10f;           // 傷害值
    public float knockbackForce = 5f;    // 擊退力量
    public LayerMask targetLayer;        // 目標 Layer 過濾

    [Header("視覺與擴散參數")]
    public float expandSpeed = 10f;      // 擴張速度
    public float maxRange = 5f;          // 最大範圍（localScale.x）
    public float fadeSpeed = 2f;         // 透明度淡出速度
    public Color waveColor = Color.cyan; // 波形顏色

    [Header("碰到指定 Tag 的行為")]
    public bool destroyMonsters = true;      // 若為 true，碰到指定 Tag 時刪除該物件
    public string monsterTag = "Monster";    // 要刪除的目標 Tag（可在 Inspector 修改）

    // 內部欄位
    private Material mat;
    private Color currentColor;
    private SphereCollider col;
    private Rigidbody rb;
    private List<GameObject> hitList = new List<GameObject>(); // 已命中的物件清單，避免重複處理

    void Start()
    {
        // 設定 Collider 為 Trigger 並初始化半徑
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.1f;

        // 確保有 Rigidbody，並不受物理影響（只用於觸發判定）
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // 初始縮放與材質設定
        transform.localScale = Vector3.zero;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            mat.color = waveColor;
            currentColor = waveColor;

            // 若材質支援 emission，就嘗試設定
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", waveColor * 2f);
        }
    }

    void Update()
    {
        // 擴張
        float growth = expandSpeed * Time.deltaTime;
        transform.localScale += Vector3.one * growth;

        // 逐漸淡出顏色透明度
        if (mat != null)
        {
            currentColor.a -= fadeSpeed * Time.deltaTime;
            mat.color = currentColor;
        }

        // 超過範圍或完全透明時銷毀自己
        if (transform.localScale.x >= maxRange || currentColor.a <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 優先以 Tag 刪除（不受 LayerMask 約束），方便將 Prefab 標記為 Monster 即被刪除
        if (destroyMonsters && other.CompareTag(monsterTag))
        {
            Debug.Log($"Shockwave：刪除目標 {other.gameObject.name}（Tag={monsterTag}）");
            Destroy(other.gameObject);
            return;
        }

        // 若未以 Tag 刪除，才用 LayerMask 過濾
        if (((1 << other.gameObject.layer) & targetLayer) == 0) return;

        // 若已處理過則忽略
        if (hitList.Contains(other.gameObject)) return;

        hitList.Add(other.gameObject);

        // 否則套用傷害或擊退
        ApplyDamage(other.gameObject);
    }

    void ApplyDamage(GameObject target)
    {
        Debug.Log($"Shockwave：對 {target.name} 造成傷害處理（傷害 {damage}）");

        // 範例：嘗試呼叫 EnemyHealth（如有）來扣血
        // var health = target.GetComponent<EnemyHealth>();
        // if (health != null) health.TakeDamage(damage);

        // 若有 Rigidbody 則加上擊退力
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            targetRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}