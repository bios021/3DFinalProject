using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;

    // 1. 加這把鎖！預設是 false (沒被撿過)
    private bool isPickedUp = false; 

    private void OnTriggerEnter(Collider other)
    {
        // 2. 如果鎖已經鎖上了，代表剛剛已經有人撿走我了，直接無視這次碰撞
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            InventoryManager manager = FindObjectOfType<InventoryManager>();
            if (manager != null)
            {
                // 嘗試加入包包
                bool success = manager.AddItem(itemData);
                
                // 只有真的成功加入包包，才鎖起來並銷毀
                if (success)
                {
                    // 3. 鎖上！防止 0.01 秒後的第二次觸發
                    isPickedUp = true; 
                    
                    Destroy(gameObject);
                }
            }
        }
    }
}