using UnityEngine;
using System.Collections.Generic; // 需要這個來紀錄打過誰

[RequireComponent(typeof(SphereCollider))] // 強制要求要有圓形碰撞體
public class Shockwave : MonoBehaviour
{
    [Header(" 震波基礎屬性")]
    public float damage = 10f;           // 傷害值
    public float knockbackForce = 5f;    // 擊退力道
    public LayerMask targetLayer;        // 震波會打到誰 (例如 Enemy)

    [Header(" 視覺與範圍屬性")]
    public float expandSpeed = 10f;      // 擴散速度
    public float maxRange = 5f;          // 最大半徑
    public float fadeSpeed = 2f;         // 消失速度
    public Color waveColor = Color.cyan; // 震波顏色

    // 內部變數
    private Material mat;
    private Color currentColor;
    private SphereCollider col;
    private List<GameObject> hitList = new List<GameObject>(); // 防止同一個敵人被打兩次

    void Start()
    {
        // 1. 初始化碰撞體
        col = GetComponent<SphereCollider>();
        col.isTrigger = true; // 設定為觸發器，才不會把人撞飛卡住
        col.radius = 0.1f;    // 一開始很小

        // 2. 初始化視覺
        transform.localScale = Vector3.zero; // 從0開始
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            // 設定初始顏色
            mat.color = waveColor;
            currentColor = waveColor;

            // 如果材質有 Emission (發光)，也可以設定
            mat.SetColor("_EmissionColor", waveColor * 2f);
        }
    }

    void Update()
    {
        // 1. 擴大震波 (視覺 + 碰撞範圍)
        float growth = expandSpeed * Time.deltaTime;

        // 視覺變大
        transform.localScale += Vector3.one * growth;

        // 2. 處理淡出
        if (mat != null)
        {
            currentColor.a -= fadeSpeed * Time.deltaTime;
            mat.color = currentColor;
        }

        // 3. 銷毀條件 (超過最大範圍 或 完全透明)
        if (transform.localScale.x >= maxRange || currentColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }

    // 當震波碰到東西時
    void OnTriggerEnter(Collider other)
    {
        // 1. 檢查是否在目標圖層內 (例如只打敵人)
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            // 2. 檢查這個敵人是否已經被打過了 (避免震波經過時每幀都造成傷害)
            if (!hitList.Contains(other.gameObject))
            {
                hitList.Add(other.gameObject); // 加入名單
                ApplyDamage(other.gameObject);
            }
        }
    }

    void ApplyDamage(GameObject target)
    {
        // 這裡實作您的傷害邏輯，例如：
        Debug.Log($"震波擊中了 {target.name}，造成 {damage} 點傷害！");

        // 範例：如果敵人身上有 EnemyHealth 腳本
        // var health = target.GetComponent<EnemyHealth>();
        // if(health != null) health.TakeDamage(damage);

        // 範例：施加擊退力 (Knockback)
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 計算擊退方向：從震波中心往敵人方向推
            Vector3 direction = (target.transform.position - transform.position).normalized;
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}