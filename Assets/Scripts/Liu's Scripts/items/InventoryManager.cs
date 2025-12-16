using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;
    private int selectedSlotIndex = 0;

    void Start()
    {
        SelectSlot(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    void SelectSlot(int index)
    {
        // 【新增】檢查目標格子是否為空
        if (slots[index].IsEmpty())
        {
            Debug.Log($"第 {index + 1} 格是空的,無法選取!");
            return; // 如果是空的就不切換
        }

        // 取消舊的選取
        slots[selectedSlotIndex].Deselect();

        // 更新索引
        selectedSlotIndex = index;

        // 選取新的格子
        slots[selectedSlotIndex].Select();

        // 【新增】顯示選中的道具提示
        Debug.Log($"選中了: {slots[selectedSlotIndex].GetItemName()}");
    }

    public bool AddItem(ItemData newItem)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].AddItem(newItem);
                
                // 【新增】如果是第一個道具,自動選中它
                if (i == 0 && selectedSlotIndex == 0)
                {
                    slots[0].Select();
                    Debug.Log($"撿到道具: {newItem.itemName}");
                }
                
                return true;
            }
        }
        Debug.Log("包包滿了!");
        return false;
    }
}