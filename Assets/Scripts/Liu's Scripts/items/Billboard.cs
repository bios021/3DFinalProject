using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // 讓這個物件的 "正面 (forward)" 永遠對齊攝影機的 "正面"
        transform.forward = Camera.main.transform.forward;
    }
}