using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;       // 拉進來：顯示圖案的 Image
    public GameObject highlight;  // 拉進來：選取框框的物件

    private ItemData currentItem; // 現在這個格子裝了什麼

    // 當撿到道具時，更新這個格子的顯示
    public void AddItem(ItemData newItem)
    {
        currentItem = newItem;
        iconImage.sprite = newItem.icon;
        iconImage.enabled = true; // 顯示圖片
    }

    // 清空格子用
    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false; // 隱藏圖片
    }

    // 當被選中時 (按下 12345)
    public void Select()
    {
        highlight.SetActive(true);
    }

    // 當取消選中時
    public void Deselect()
    {
        highlight.SetActive(false);
    }
    
    // 讓外部知道這個格子是不是空的
    public bool IsEmpty()
    {
        return currentItem == null;
    }
}