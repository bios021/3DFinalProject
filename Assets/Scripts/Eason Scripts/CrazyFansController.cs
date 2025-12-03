using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrazyFansController : MonoBehaviour
{
    [Header("中心點（可在 Inspector 指定）")]
    public Transform stageCenter;

    [Header("半徑設定")]
    public float outerRadius = 15f;    // 第一個圓半徑
    public float innerRadius = 8f;  // 第二個圓半徑（內圈）

    [Header("要隨機生成的 Prefab（請在 Inspector 指定）")]
    public GameObject CrazyFan_Purple;
    public GameObject Character_Gray;
    public GameObject Character_Green;
    public GameObject Character_Pink;

    [Header("生成控制")]
    public bool spawnOnStart = true;
    [Tooltip("如果 spawnOnStart=true，會在 Start 時生成此數量")]
    public int spawnCount = 10;
    [Tooltip("在指定秒數內均勻產生 spawnCount 個（預設 10 秒）")]
    public float spawnDuration = 10f;

    // 可選：把產生的物件以本物件為父物件，便於階層管理
    public bool parentToThis = true;

    private List<GameObject> prefabs = new List<GameObject>();
    private Coroutine spawnCoroutine;

    void Awake()
    {
        // 建立可用 prefab 列表（忽略為 null 的）
        prefabs.Clear();
        if (CrazyFan_Purple != null) prefabs.Add(CrazyFan_Purple);
        if (Character_Gray != null) prefabs.Add(Character_Gray);
        if (Character_Green != null) prefabs.Add(Character_Green);
        if (Character_Pink != null) prefabs.Add(Character_Pink);
    }

    void Start()
    {
        if (spawnOnStart)
        {
            // 如果 spawnDuration <= 0 或 spawnCount <= 0 則回退為立刻生成
            if (spawnCount <= 0 || spawnDuration <= 0f)
            {
                SpawnMultiple(spawnCount);
            }
            else
            {
                // 在 spawnDuration 秒內均勻生成 spawnCount 個（例如 spawnDuration=10, spawnCount=10 => 每秒1個）
                spawnCoroutine = StartCoroutine(SpawnOverTime(spawnCount, spawnDuration));
            }
        }
    }

    // 在 duration 秒內均勻生成 count 個
    private IEnumerator SpawnOverTime(int count, float duration)
    {
        if (stageCenter == null)
        {
            Debug.LogWarning("CrazyFansController: stageCenter 未指定，停止生成。");
            yield break;
        }

        if (prefabs.Count == 0)
        {
            Debug.LogWarning("CrazyFansController: 沒有可用的 Prefab，請在 Inspector 指定。");
            yield break;
        }

        if (innerRadius < 0f) innerRadius = 0f;
        if (outerRadius <= innerRadius)
        {
            Debug.LogWarning("CrazyFansController: outerRadius 必須大於 innerRadius，已調整為 innerRadius + 0.1f");
            outerRadius = innerRadius + 0.1f;
        }

        if (count <= 0)
            yield break;

        float interval = duration / count;
        for (int i = 0; i < count; i++)
        {
            SpawnOne();
            // 若最後一個不必等待則可檢查 i < count -1，但此處保持一致間隔
            yield return new WaitForSeconds(interval);
        }

        spawnCoroutine = null;
    }

    // 生成指定數量（立即生成）
    public void SpawnMultiple(int count)
    {
        if (stageCenter == null)
        {
            Debug.LogWarning("CrazyFansController: stageCenter 未指定。");
            return;
        }

        if (prefabs.Count == 0)
        {
            Debug.LogWarning("CrazyFansController: 沒有可用的 Prefab，請在 Inspector 指定。");
            return;
        }

        if (innerRadius < 0f) innerRadius = 0f;
        if (outerRadius <= innerRadius)
        {
            Debug.LogWarning("CrazyFansController: outerRadius 必須大於 innerRadius，已調整為 innerRadius + 0.1f");
            outerRadius = innerRadius + 0.1f;
        }

        for (int i = 0; i < count; i++)
        {
            SpawnOne();
        }
    }

    // 單個位置生成
    public GameObject SpawnOne()
    {
        if (stageCenter == null) return null;
        if (prefabs.Count == 0) return null;

        Vector3 pos = RandomPointInAnnulus(stageCenter.position, innerRadius, outerRadius);

        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
        GameObject go = Instantiate(prefab, pos, Quaternion.identity, parentToThis ? transform : null);
        return go;
    }

    // 在兩個半徑之間均勻取樣（XZ 平面），Y 以 center.y 為準
    private Vector3 RandomPointInAnnulus(Vector3 center, float rInner, float rOuter)
    {
        // 均勻取樣環面半徑： r = sqrt( u*(R^2 - r^2) + r^2 )
        float rInnerSq = rInner * rInner;
        float rOuterSq = rOuter * rOuter;
        float u = Random.value;
        float r = Mathf.Sqrt(Mathf.Lerp(rInnerSq, rOuterSq, u));

        float theta = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Cos(theta) * r;
        float z = Mathf.Sin(theta) * r;

        return new Vector3(center.x + x, center.y, center.z + z);
    }

#if UNITY_EDITOR
    // 在 Scene 視窗中繪製兩個圓（選取時），便於調整
    void OnDrawGizmosSelected()
    {
        if (stageCenter == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        DrawCircle(stageCenter.position, outerRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        DrawCircle(stageCenter.position, innerRadius);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        const int segments = 60;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float ang = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
