using UnityEngine;

/// <summary>
/// 간단한 VR 카메라 컨트롤러 (PC 테스트용)
/// WASD: 이동, 마우스: 회전
/// </summary>
public class SimpleVRController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float lookSpeed = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Start()
    {
        // 마우스 커서 숨기기 (ESC로 해제)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleCursor();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");  // A, D
        float v = Input.GetAxis("Vertical");    // W, S

        Vector3 move = transform.right * h + transform.forward * v;
        transform.position += move * moveSpeed * Time.deltaTime;

        // 위/아래 이동 (Q, E)
        if (Input.GetKey(KeyCode.Q)) transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        rotationY += mouseX;

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    private void HandleCursor()
    {
        // ESC로 마우스 커서 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }
}
