using UnityEngine;

public class SmoothPlayerController : MonoBehaviour
{
    // 基础参数
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    // 平滑过渡参数
    public float moveSmoothTime = 3.7f;
    public float mouseSmoothTime = 0.38f;

    // 私有变量
    private float rotationX = 0f;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector2 currentMouseVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;

    void Start()
    {
        // 锁定并隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 平滑处理鼠标输入
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseVelocity, mouseSmoothTime);
        
        // 处理视角旋转
        rotationX -= currentMouseDelta.y * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y + currentMouseDelta.x * mouseSensitivity, 0f);

        // 平滑处理移动输入
        Vector3 targetVelocity = new Vector3(
            Input.GetAxis("Horizontal") * moveSpeed,
            0,
            Input.GetAxis("Vertical") * moveSpeed
        );
        
        // 使用 SmoothDamp 实现平滑移动
        Vector3 smoothVelocity = Vector3.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref currentVelocity,
            moveSmoothTime
        );

        // 应用移动
        transform.Translate(smoothVelocity * Time.deltaTime);
    }
}