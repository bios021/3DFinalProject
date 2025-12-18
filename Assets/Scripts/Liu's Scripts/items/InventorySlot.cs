using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    public GameObject highlight;
    private ItemData currentItem;

    public void AddItem(ItemData newItem)
    {
        currentItem = newItem;
        iconImage.sprite = newItem.icon;
        iconImage.enabled = true;
        iconImage.color = new Color(1, 1, 1, 1);
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    public void Select()
    {
        highlight.SetActive(true);
    }

    public void Deselect()
    {
        highlight.SetActive(false);
    }
    
    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public string GetItemName()
    {
        return currentItem != null ? currentItem.itemName : "空格子";
    }

    // 【新增】讓外部可以取得這個格子裡的完整道具資料
    public ItemData GetItem()
    {
        return currentItem;
    }
}