using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData; // 在這裡指定這是什麼道具

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 確保是玩家碰到
        {
            // 找到場景上的 InventoryManager (也可以用單例模式，這邊簡單用 Find)
            InventoryManager manager = FindObjectOfType<InventoryManager>();
            
            // 試著把道具放進去
            if (manager.AddItem(itemData))
            {
                Destroy(gameObject); // 撿起來後，刪除地上的模型
            }
        }
    }
}