using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    
    [Header("滑鼠視角設定")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f; // 上下視角限制
    
    [Header("參考物件")]
    public Transform cameraTransform; // 把相機拖進來
    
    // 私有變數
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    
    void Start()
    {
        // 取得 CharacterController 組件
        controller = GetComponent<CharacterController>();
        
        // 如果沒有手動指定相機，自動尋找主相機
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // 鎖定並隱藏滑鼠游標
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // 檢查是否在地面上
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 確保穩定貼地
        }
        
        // 處理滑鼠視角
        HandleMouseLook();
        
        // 處理移動
        HandleMovement();
        
        // 處理跳躍
        HandleJump();
        
        // 應用重力
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // 按 ESC 解鎖滑鼠
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void HandleMouseLook()
    {
        // 取得滑鼠輸入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 上下視角 (pitch) - 控制相機
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // 左右旋轉 (yaw) - 控制角色本體
        transform.Rotate(Vector3.up * mouseX);
    }
    
    void HandleMovement()
    {
        // 取得輸入
        float x = Input.GetAxis("Horizontal"); // A/D 或左右鍵
        float z = Input.GetAxis("Vertical");   // W/S 或上下鍵
        
        // 計算移動方向（相對於角色面向）
        Vector3 move = transform.right * x + transform.forward * z;
        
        // 判斷是否在衝刺
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        
        // 移動角色
        controller.Move(move * currentSpeed * Time.deltaTime);
    }
    
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
}