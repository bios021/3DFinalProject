using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;       // 顯示 Icon 的圖片
    public GameObject highlight;  // 選取框框

    private ItemData currentItem; // 這裡面裝什麼

    // 【這裡改了！】當撿到道具時，立刻顯示！
    public void AddItem(ItemData newItem)
    {
        currentItem = newItem;
        
        // 1. 設定圖片
        iconImage.sprite = newItem.icon;
        
        // 2. 關鍵：把圖片組件打開 (之前可能預設是關的)
        iconImage.enabled = true; 

        // 3. 保險起見：確保顏色是「白色且不透明」
        // 有時候顏色會變成透明的，導致有圖也看不到
        iconImage.color = new Color(1, 1, 1, 1); 
    }

    // 當清空格子時
    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        
        // 關鍵：沒東西時把圖片關掉，不然會看到一個白方塊
        iconImage.enabled = false; 
    }

    // 當被選中時 (只負責處理框框，不要處理 Icon)
    public void Select()
    {
        highlight.SetActive(true);
    }

    // 當取消選中時
    public void Deselect()
    {
        highlight.SetActive(false);
    }
    
    public bool IsEmpty()
    {
        return currentItem == null;
    }
}