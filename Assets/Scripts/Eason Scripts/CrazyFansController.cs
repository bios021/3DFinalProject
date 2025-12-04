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
        RebuildPrefabList();
    }

    void Start()
    {
        // 如果未在 Inspector 指派 stageCenter，嘗試自動尋找場景中的 StageCenter
        if (stageCenter == null)
        {
            // 優先用 Tag 查找（請在場景中將中心物件設 Tag = "StageCenter"）
            GameObject found = null;
            try
            {
                found = GameObject.FindWithTag("StageCenter");
            }
            catch (UnityException)
            {
                // 如果 Tag 不存在，FindWithTag 會拋出例外，這裡忽略並改用名稱搜尋
                found = null;
            }

            if (found == null)
            {
                // 若沒設定 Tag，改用名稱查找（場景物件名稱為 "StageCenter"）
                found = GameObject.Find("StageCenter");
            }

            if (found != null)
            {
                stageCenter = found.transform;
                Debug.Log($"CrazyFansController: 自動找到 StageCenter -> {found.name}");
            }
            else
            {
                Debug.LogWarning("CrazyFansController: 未指定 stageCenter，且場景中找不到 Tag 或 Name 為 'StageCenter' 的物件。請在 Inspector 指定或在場景建立 StageCenter（並設定 Tag）。");
            }
        }

        RebuildPrefabList();
        Debug.Log($"CrazyFansController.Start: 可用 prefabs = {prefabs.Count}");
        for (int i = 0; i < prefabs.Count; i++)
        {
            Debug.Log($"  prefab[{i}] = {(prefabs[i] != null ? prefabs[i].name : "null")}");
        }

        if (spawnOnStart)
        {
            if (spawnCount <= 0 || spawnDuration <= 0f)
            {
                SpawnMultiple(spawnCount);
            }
            else
            {
                spawnCoroutine = StartCoroutine(SpawnOverTime(spawnCount, spawnDuration));
            }
        }
    }

    // 重新建立 internal prefab 清單（過濾 null 並在 Editor 下額外檢查場景物件）
    private void RebuildPrefabList()
    {
        prefabs.Clear();
        if (CrazyFan_Purple != null) prefabs.Add(CrazyFan_Purple);
        if (Character_Gray != null) prefabs.Add(Character_Gray);
        if (Character_Green != null) prefabs.Add(Character_Green);
        if (Character_Pink != null) prefabs.Add(Character_Pink);

        prefabs.RemoveAll(p => p == null);

#if UNITY_EDITOR
        foreach (var p in new List<GameObject>(prefabs))
        {
            if (p == null) continue;
            // 若為場景物件，提醒（場景物件在執行時被 Destroy 會造成 MissingReferenceException）
            if (p.scene.IsValid())
            {
                Debug.LogWarning($"CrazyFansController: 欄位使用了場景中的物件 ({p.name}) 而非 Prefab 資產。請在 Project 視窗拖入 Prefab（藍色）。");
            }
        }
#endif
    }

    private IEnumerator SpawnOverTime(int count, float duration)
    {
        if (stageCenter == null)
        {
            Debug.LogWarning("CrazyFansController: stageCenter 未指定，停止生成。");
            yield break;
        }

        RebuildPrefabList();
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

        if (count <= 0) yield break;

        float interval = duration / count;
        for (int i = 0; i < count; i++)
        {
            RebuildPrefabList();
            if (prefabs.Count == 0)
            {
                Debug.LogWarning("CrazyFansController: 執行時沒有可用的 Prefab，停止後續生成。");
                yield break;
            }

            SpawnOne();
            yield return new WaitForSeconds(interval);
        }

        spawnCoroutine = null;
    }

    public void SpawnMultiple(int count)
    {
        if (stageCenter == null)
        {
            Debug.LogWarning("CrazyFansController: stageCenter 未指定。");
            return;
        }

        RebuildPrefabList();
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

    // 加強除錯與保護的 SpawnOne
    public GameObject SpawnOne()
    {
        if (stageCenter == null) return null;

        RebuildPrefabList();
        if (prefabs.Count == 0)
        {
            Debug.LogWarning("CrazyFansController.SpawnOne: 沒有可用的 Prefab，跳過生成。");
            return null;
        }

        Vector3 pos = RandomPointInAnnulus(stageCenter.position, innerRadius, outerRadius);

        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
        if (prefab == null)
        {
            Debug.LogWarning("CrazyFansController.SpawnOne: 隨機選到的 prefab 為 null，跳過生成。");
            return null;
        }

#if UNITY_EDITOR
        if (prefab.scene.IsValid())
        {
            Debug.LogWarning($"CrazyFansController: 嘗試 Instantiate 場景物件 {prefab.name}（不是 Prefab 資產）。請改用 Project 裡的 Prefab。");
            return null;
        }
#endif

        // 詳細日誌，方便追蹤 Instantiate 是否發生例外
        Debug.Log($"CrazyFansController: Instantiate {prefab.name} at {pos} parentToThis={parentToThis}");

        try
        {
            GameObject go = Instantiate(prefab, pos, Quaternion.identity, parentToThis ? transform : null);

            // 指派 stageCenter 給 CrazyFan 腳本（root 或子物件）
            var fan = go.GetComponent<CrazyFan>();
            if (fan == null) fan = go.GetComponentInChildren<CrazyFan>();
            if (fan != null)
            {
                fan.stageCenter = stageCenter;
            }

            return go;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"CrazyFansController: Instantiate 失敗，Prefab 名稱 = {prefab.name}. Exception: {ex}");
            return null;
        }
    }

    private Vector3 RandomPointInAnnulus(Vector3 center, float rInner, float rOuter)
    {
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
