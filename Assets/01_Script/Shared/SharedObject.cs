using UnityEngine;
using Photon.Pun;

/// <summary>
/// 네트워크 동기화 오브젝트
/// AR/VR 플랫폼에 맞게 좌표 변환하여 표시
/// </summary>
public class SharedObject : MonoBehaviourPun, IPunObservable
{
    [Header("Sync Settings")]
    [SerializeField] private float positionLerpSpeed = 10f;
    [SerializeField] private float rotationLerpSpeed = 10f;

    // 네트워크에서 받은 좌표 (앵커 기준 상대 좌표)
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkScale;

    // 보간용
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Start()
    {
        // 초기값 설정
        networkPosition = Vector3.zero;
        networkRotation = Quaternion.identity;
        networkScale = transform.localScale;

        if (photonView.IsMine)
        {
            // 내 오브젝트면 현재 위치를 네트워크 좌표로 변환
            UpdateNetworkPosition();
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            // 내 오브젝트: 로컬 좌표를 네트워크 좌표로 변환하여 전송
            UpdateNetworkPosition();
        }
        else
        {
            // 다른 사람 오브젝트: 네트워크 좌표를 로컬 좌표로 변환하여 표시
            ApplyNetworkPosition();
        }
    }

    /// <summary>
    /// 로컬 위치를 네트워크 좌표로 변환
    /// </summary>
    private void UpdateNetworkPosition()
    {
        if (XRPlatformManager.Instance != null)
        {
            networkPosition = XRPlatformManager.Instance.LocalToNetworkPosition(transform.position);
            networkRotation = XRPlatformManager.Instance.LocalToNetworkRotation(transform.rotation);
        }
        else
        {
            networkPosition = transform.position;
            networkRotation = transform.rotation;
        }
        networkScale = transform.localScale;
    }

    /// <summary>
    /// 네트워크 좌표를 로컬 위치로 변환하여 적용
    /// </summary>
    private void ApplyNetworkPosition()
    {
        if (XRPlatformManager.Instance != null)
        {
            targetPosition = XRPlatformManager.Instance.NetworkToLocalPosition(networkPosition);
            targetRotation = XRPlatformManager.Instance.NetworkToLocalRotation(networkRotation);
        }
        else
        {
            targetPosition = networkPosition;
            targetRotation = networkRotation;
        }

        // 부드러운 보간
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        transform.localScale = networkScale;
    }

    /// <summary>
    /// Photon 동기화 콜백
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송 (내 오브젝트)
            stream.SendNext(networkPosition);
            stream.SendNext(networkRotation);
            stream.SendNext(networkScale);
        }
        else
        {
            // 데이터 수신 (다른 사람 오브젝트)
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkScale = (Vector3)stream.ReceiveNext();
        }
    }

    /// <summary>
    /// 오브젝트 이동 (로컬에서 호출)
    /// </summary>
    public void Move(Vector3 delta)
    {
        if (photonView.IsMine)
        {
            transform.position += delta;
        }
    }

    /// <summary>
    /// 오브젝트 회전 (로컬에서 호출)
    /// </summary>
    public void Rotate(Vector3 eulerAngles)
    {
        if (photonView.IsMine)
        {
            transform.Rotate(eulerAngles);
        }
    }

    /// <summary>
    /// 오브젝트 스케일 변경 (로컬에서 호출)
    /// </summary>
    public void Scale(float scaleFactor)
    {
        if (photonView.IsMine)
        {
            transform.localScale *= scaleFactor;
        }
    }

    /// <summary>
    /// RPC로 상태 변경 동기화 (예: 색상 변경 등)
    /// </summary>
    [PunRPC]
    public void SyncState(string stateName, string value)
    {
        Debug.Log($"[SharedObject] 상태 동기화: {stateName} = {value}");
        // 필요에 따라 상태 처리 추가
    }

    /// <summary>
    /// 상태 변경 브로드캐스트
    /// </summary>
    public void BroadcastState(string stateName, string value)
    {
        photonView.RPC("SyncState", RpcTarget.All, stateName, value);
    }
}
