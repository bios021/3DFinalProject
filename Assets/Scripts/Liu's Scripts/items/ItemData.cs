using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName; // 道具名稱
    public Sprite icon;     // 道具在 UI 顯示的圖片
    public GameObject prefab; // (選填) 如果你要丟出來，丟出來的模型
}