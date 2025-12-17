using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;
    private int selectedSlotIndex = 0;

    void Start()
    {
        SelectSlot(0); // 遊戲開始選第一格
    }

    void Update()
    {
        // 你的按鍵邏輯...
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    void SelectSlot(int index)
    {
        // 防呆：避免超出陣列範圍
        if (index < 0 || index >= slots.Length) return;

        slots[selectedSlotIndex].Deselect(); // 關舊的框框
        selectedSlotIndex = index;
        slots[selectedSlotIndex].Select();   // 開新的框框
    }

    public bool AddItem(ItemData newItem)
    {
        // 迴圈：從第 0 格檢查到第 4 格
        for (int i = 0; i < slots.Length; i++)
        {
            // 找到第一個空格
            if (slots[i].IsEmpty())
            {
                slots[i].AddItem(newItem); // 放進去
                return true; // 回報成功，並立刻結束這個函式！(重要)
            }
        }
        
        Debug.Log("包包滿了！");
        return false; // 包包滿了，回報失敗
    }
}