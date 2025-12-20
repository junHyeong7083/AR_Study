using UnityEngine;
using Photon.Pun;

/// <summary>
/// 오브젝트 상호작용 (회전, 스케일)
/// AR: 터치 드래그로 회전
/// VR: 버튼으로 회전
/// </summary>
public class ObjectInteraction : MonoBehaviourPun
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float touchRotationSpeed = 0.5f;
    [SerializeField] private float buttonRotationAmount = 15f;  // 버튼 클릭당 회전량

    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private float scaleSpeed = 0.1f;

    private bool isDragging = false;
    private Vector2 lastTouchPosition;

    private void Update()
    {
        // AR 모드에서만 터치 회전
        if (XRPlatformManager.Instance != null && XRPlatformManager.Instance.IsAR)
        {
            HandleTouchRotation();
        }
    }

    #region Touch Rotation (AR)

    private void HandleTouchRotation()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // 터치 시작 - 오브젝트에 닿았는지 확인
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        isDragging = true;
                        lastTouchPosition = touch.position;
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                // 드래그 중 - 회전
                Vector2 delta = touch.position - lastTouchPosition;
                RotateObject(delta.x * touchRotationSpeed);
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

        // 핀치 줌 (스케일)
        if (Input.touchCount == 2)
        {
            HandlePinchZoom();
        }
    }

    private void HandlePinchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
        float currentMagnitude = (touch0.position - touch1.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;

        ScaleObject(difference * scaleSpeed * 0.01f);
    }

    #endregion

    #region Button Controls (VR & AR)

    /// <summary>
    /// 왼쪽으로 회전 (버튼용)
    /// </summary>
    public void RotateLeft()
    {
        RotateObject(-buttonRotationAmount);
    }

    /// <summary>
    /// 오른쪽으로 회전 (버튼용)
    /// </summary>
    public void RotateRight()
    {
        RotateObject(buttonRotationAmount);
    }

    /// <summary>
    /// 크게 (버튼용)
    /// </summary>
    public void ScaleUp()
    {
        ScaleObject(scaleSpeed);
    }

    /// <summary>
    /// 작게 (버튼용)
    /// </summary>
    public void ScaleDown()
    {
        ScaleObject(-scaleSpeed);
    }

    #endregion

    #region Core Functions

    private void RotateObject(float angle)
    {
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            transform.Rotate(Vector3.up, angle, Space.World);

            // 네트워크로 동기화 (RPC)
            photonView.RPC("SyncRotation", RpcTarget.Others, transform.rotation);
        }
    }

    private void ScaleObject(float delta)
    {
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            Vector3 newScale = transform.localScale + Vector3.one * delta;
            newScale = Vector3.Max(newScale, Vector3.one * minScale);
            newScale = Vector3.Min(newScale, Vector3.one * maxScale);
            transform.localScale = newScale;

            // 네트워크로 동기화 (RPC)
            photonView.RPC("SyncScale", RpcTarget.Others, transform.localScale);
        }
    }

    [PunRPC]
    private void SyncRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    [PunRPC]
    private void SyncScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    #endregion
}
