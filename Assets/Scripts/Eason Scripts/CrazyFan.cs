using UnityEngine;

public class CrazyFan : MonoBehaviour
{
    public Transform stageCenter;   // 拖舞台中心或玩家進來
    public float rushSpeed = 2f;
    public float damage = 10f;

    void Update()
    {
        if (stageCenter == null) return;

        // 確保自己固定在 Y = 0
        Vector3 current = transform.position;
        current.y = 0f;
        transform.position = current;

        // 只取舞台的 XZ，目標 Y 固定為 0（或使用 transform.position.y）
        Vector3 target = new Vector3(stageCenter.position.x, 0f, stageCenter.position.z);

        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
        {
            dir.Normalize();
            transform.position += dir * rushSpeed * Time.deltaTime;

            // 保持在同一高度看向舞台（避免上下俯仰）
            Vector3 lookTarget = new Vector3(stageCenter.position.x, transform.position.y, stageCenter.position.z);
            transform.LookAt(lookTarget);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 這裡之後接你的傷害系統
            Debug.Log("瘋狂粉絲撲上來啦！！");
            Destroy(gameObject);
        }
    }
}