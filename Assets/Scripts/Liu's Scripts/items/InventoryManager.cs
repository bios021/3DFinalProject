using UnityEngine;
using TMPro; // 引用 TextMeshPro

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;
    
    [Header("UI 提示")]
    public TMP_Text hintText; // 拖入 UI 上的 TextMeshPro 文字物件

    [Header("繩子設定")]
    public LineRenderer ropeLineRenderer; // 拖入一個帶有 LineRenderer 的空物件 (可選)
    private NormalBean currentDraggedBean; // 目前正在牽著的豆子

    private int selectedSlotIndex = 0;

    void Start()
    {
        SelectSlot(0);
    }

    void Update()
    {
        // 數字鍵切換
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);

        // 按 F 使用道具
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseCurrentItem();
        }

        // 【新增】持續更新提示文字
        UpdateHintUI();

        // 【新增】如果正在牽著豆子，更新繩子視覺效果
        if (currentDraggedBean != null && ropeLineRenderer != null)
        {
            // 起點：玩家位置 (稍微往下偏一點，像是在手上)
            ropeLineRenderer.SetPosition(0, Camera.main.transform.position + Vector3.down * 0.5f);
            // 終點：豆子位置
            ropeLineRenderer.SetPosition(1, currentDraggedBean.transform.position);
        }
        else if (ropeLineRenderer != null)
        {
            // 沒牽東西時隱藏繩子
            ropeLineRenderer.positionCount = 0;
        }
    }

    void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        slots[selectedSlotIndex].Deselect();
        selectedSlotIndex = index;
        slots[selectedSlotIndex].Select();
    }

    // 【新增】根據當前道具顯示提示
    void UpdateHintUI()
    {
        if (hintText == null) return;

        InventorySlot currentSlot = slots[selectedSlotIndex];
        ItemData item = currentSlot.GetItem();

        if (item == null)
        {
            hintText.text = ""; // 空格子不顯示提示
        }
        else
        {
            switch (item.itemName)
            {
                case "Cake":
                    hintText.text = "press F to gain 1 health";
                    break;
                case "Gun":
                    hintText.text = "aim Monster to Freeze it";
                    break;
                case "Rope": // 假設道具名稱叫 "Rope"
                    if (currentDraggedBean == null)
                        hintText.text = "aim NormalBean to Drag it";
                    else
                        hintText.text = "press F to Release";
                    break;
                default:
                    hintText.text = $"press F user {item.itemName}";
                    break;
            }
        }
    }

    public bool AddItem(ItemData newItem)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].AddItem(newItem);
                return true;
            }
        }
        Debug.Log("包包滿了！");
        return false;
    }

    void UseCurrentItem()
    {
        InventorySlot currentSlot = slots[selectedSlotIndex];
        ItemData item = currentSlot.GetItem();

        if (item != null)
        {
            // 執行功能
            bool success = ApplyItemEffect(item);

            // 只有執行成功才消耗道具 (例如槍沒打到人就不消耗，看你需求)
            // 這裡假設只要按了就消耗
            if (success)
            {
                currentSlot.ClearSlot();
            }
        }
    }

    // 修改：回傳 bool 表示是否成功使用
    bool ApplyItemEffect(ItemData item)
    {
        switch (item.itemName)
        {
            case "Cake":
                Debug.Log("吃了蛋糕，回復體力！");
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    player.Heal(1); // 回復 1 點血
                    return true;
                }
                break;

            case "Gun":
                Debug.Log("發射凍結槍！");
                return FireFreezeGun(); // 呼叫發射邏輯

            case "Rope":
                return ToggleRope(); // 切換牽引狀態

            default:
                Debug.Log($"道具 {item.itemName} 還沒有實作功能");
                return true;
        }
        return false;
    }

    // 【新增】槍的發射邏輯
    bool FireFreezeGun()
    {
        // 取得玩家相機 (作為發射起點)
        Camera playerCam = Camera.main;
        if (playerCam == null) return false;

        // 設定判定範圍參數
        float range = 10f; // 射程
        float width = 2f;  // 寬度 (BoxCast 用)

        // 使用 BoxCast 模擬長方形範圍
        // Origin: 相機位置
        // HalfExtents: 盒子的一半大小 (寬度/2, 高度/2, 長度/0.1) -> 長度主要靠 distance 控制
        // Direction: 相機前方
        // Orientation: 相機旋轉
        // MaxDistance: 射程
        
        RaycastHit[] hits = Physics.BoxCastAll(
            playerCam.transform.position, 
            new Vector3(width / 2, width / 2, 0.1f), 
            playerCam.transform.forward, 
            playerCam.transform.rotation, 
            range
        );

        bool hitMonster = false;

        foreach (RaycastHit hit in hits)
        {
            // 檢查是否打到 MonsterBean
            MonsterBean monster = hit.collider.GetComponent<MonsterBean>();
            if (monster != null)
            {
                monster.Freeze(5.0f); // 凍結 5 秒
                hitMonster = true;
            }
        }

        if (hitMonster)
        {
            Debug.Log("凍結了怪物！");
        }
        else
        {
            Debug.Log("沒打中任何怪物...");
        }

        return true; // 即使沒打中也算使用成功 (消耗道具)
    }

    // 【新增】繩子邏輯
    bool ToggleRope()
    {
        // 情況 1: 已經牽著豆子 -> 放開
        if (currentDraggedBean != null)
        {
            currentDraggedBean.StopDragging();
            currentDraggedBean = null;
            
            if (ropeLineRenderer != null) ropeLineRenderer.positionCount = 0;
            
            Debug.Log("放開了豆子");
            return true; // 成功執行 (但不一定消耗道具，看你想不想讓繩子是一次性的)
                         // 如果繩子是永久道具，這裡可以回傳 false 避免被 ClearSlot() 刪除
                         // 但你的架構是 UseCurrentItem() 只要回傳 true 就會刪除
                         // 所以如果你希望繩子不消失，這裡要改一下 UseCurrentItem 的邏輯，或者讓繩子數量無限
        }

        // 情況 2: 沒牽著豆子 -> 嘗試抓取
        Camera playerCam = Camera.main;
        if (playerCam == null) return false;

        float range = 10f;
        float width = 2f;

        RaycastHit[] hits = Physics.BoxCastAll(
            playerCam.transform.position, 
            new Vector3(width / 2, width / 2, 0.1f), 
            playerCam.transform.forward, 
            playerCam.transform.rotation, 
            range
        );

        foreach (RaycastHit hit in hits)
        {
            NormalBean bean = hit.collider.GetComponent<NormalBean>();
            // 確保抓到的是 NormalBean 且它還活著
            if (bean != null && !bean.isDead)
            {
                currentDraggedBean = bean;
                currentDraggedBean.StartDragging(playerCam.transform); // 讓它跟著相機(玩家)走
                
                // 開啟繩子視覺
                if (ropeLineRenderer != null)
                {
                    ropeLineRenderer.positionCount = 2;
                }

                Debug.Log($"抓到了 {bean.name}！");
                return true; // 成功抓取
            }
        }

        Debug.Log("沒抓到任何豆子...");
        return false;
    }
}