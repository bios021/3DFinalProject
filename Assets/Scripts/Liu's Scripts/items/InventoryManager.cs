using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots; // 把那 5 個 Slot 拉進來
    private int selectedSlotIndex = 0; // 目前選到第幾個 (0代表第1格)

    void Start()
    {
        // 遊戲開始先選第 1 格
        SelectSlot(0);
    }

    void Update()
    {
        // 監聽鍵盤輸入
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    // 處理選取邏輯
    void SelectSlot(int index)
    {
        // 1. 先把舊的選取框關掉
        slots[selectedSlotIndex].Deselect();

        // 2. 更新索引
        selectedSlotIndex = index;

        // 3. 把新的選取框打開
        slots[selectedSlotIndex].Select();
    }

    // 給外部呼叫：撿起道具
    public bool AddItem(ItemData newItem)
    {
        // 尋找第一個空的格子
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].AddItem(newItem);
                return true; // 成功放入
            }
        }
        Debug.Log("包包滿了！");
        return false; // 包包滿了
    }
}