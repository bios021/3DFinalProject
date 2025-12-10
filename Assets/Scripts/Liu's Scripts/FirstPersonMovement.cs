using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;       // 移動速度
    public float gravity = -9.81f;     // 重力
    
    [Header("視角設定")]
    public Transform playerCamera;     // 拖入你的 Main Camera
    public float mouseSensitivity = 100f; // 滑鼠靈敏度
    public float lookXLimit = 85f;     // 上下看限制的角度 (避免脖子斷掉)

    private CharacterController characterController;
    private Vector3 velocity;
    private float xRotation = 0f;      // 紀錄目前的上下角度

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // 鎖定滑鼠游標在螢幕中央並隱藏
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 如果沒有手動指定相機，自動抓取子物件中的相機
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>().transform;
        }
    }

    void Update()
    {
        // 1. 處理視角旋轉 (Look)
        HandleMouseLook();

        // 2. 處理角色移動 (Movement)
        HandleMovement();
    }

    void HandleMouseLook()
    {
        // 取得滑鼠輸入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- 上下旋轉 (只轉相機) ---
        xRotation -= mouseY;
        // 限制抬頭低頭的角度
        xRotation = Mathf.Clamp(xRotation, -lookXLimit, lookXLimit);
        
        // 套用旋轉到相機 (Local Rotation)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // --- 左右旋轉 (轉整個身體) ---
        // 身體旋轉是用 transform.Rotate，這樣前方 (Forward) 方向才會跟著改變
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // 取得鍵盤輸入 (WASD)
        float x = Input.GetAxis("Horizontal"); // A, D
        float z = Input.GetAxis("Vertical");   // W, S

        // 計算移動方向 (相對於角色的方向)
        // transform.right = 角色的右邊, transform.forward = 角色的前方
        Vector3 move = transform.right * x + transform.forward * z;

        // 執行移動
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // --- 簡單重力處理 ---
        // 如果在地面上且速度向下，重置垂直速度 (保持貼地)
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 累加重力
        velocity.y += gravity * Time.deltaTime;
        
        // 執行垂直移動 (掉落)
        characterController.Move(velocity * Time.deltaTime);
    }
}