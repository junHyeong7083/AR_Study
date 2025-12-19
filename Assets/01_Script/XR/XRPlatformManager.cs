using UnityEngine;

/// <summary>
/// AR/VR 플랫폼 분기 처리
/// 선택한 플랫폼에 따라 적절한 XR Rig 활성화
/// </summary>
public class XRPlatformManager : MonoBehaviour
{
    public static XRPlatformManager Instance { get; private set; }

    [Header("XR Rigs")]
    [SerializeField] private GameObject arRig;
    [SerializeField] private GameObject vrRig;

    [Header("Debug")]
    [SerializeField] private bool forceAR = false;
    [SerializeField] private bool forceVR = false;

    public bool IsAR { get; private set; }
    public bool IsVR => !IsAR;

    // 현재 활성화된 앵커 (AR: 마커 위치, VR: 월드 원점)
    public Transform CurrentAnchor { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePlatform();
    }

    private void InitializePlatform()
    {
        // 디버그용 강제 설정
        if (forceAR)
        {
            ActivateAR();
            return;
        }
        if (forceVR)
        {
            ActivateVR();
            return;
        }

        // RoomManager에서 선택한 플랫폼 확인
        if (RoomManager.Instance != null)
        {
            if (RoomManager.Instance.SelectedPlatform == RoomManager.PlatformType.AR)
                ActivateAR();
            else
                ActivateVR();
        }
        else
        {
            // RoomManager 없으면 기본 AR
            ActivateAR();
        }
    }

    private void PlatformSetup(bool useAR)
    {
        IsAR = useAR;
        if (arRig != null) arRig.SetActive(useAR);
        if (vrRig != null) vrRig.SetActive(!useAR);
    }

    public void ActivateAR()
    {
        PlatformSetup(true);
        Debug.Log("[XRPlatformManager] AR 모드 활성화");
    }

    public void ActivateVR()
    {
        PlatformSetup(false);

        // VR은 월드 원점을 앵커로 사용
        if (CurrentAnchor == null)
        {
            GameObject anchorObj = new GameObject("VR_WorldAnchor");
            anchorObj.transform.position = Vector3.zero;
            CurrentAnchor = anchorObj.transform;
        }

        Debug.Log("[XRPlatformManager] VR 모드 활성화");
    }

    /// <summary>
    /// AR 마커 감지 시 앵커 설정
    /// </summary>
    public void SetARMarkerAnchor(Transform markerTransform)
    {
        if (IsAR)
        {
            CurrentAnchor = markerTransform;
            Debug.Log("[XRPlatformManager] AR 마커 앵커 설정됨");
        }
    }

    /// <summary>
    /// 네트워크 좌표를 로컬 좌표로 변환
    /// </summary>
    public Vector3 NetworkToLocalPosition(Vector3 networkPosition)
    {
        if (CurrentAnchor != null)
            return CurrentAnchor.TransformPoint(networkPosition);
        return networkPosition;
    }

    /// <summary>
    /// 로컬 좌표를 네트워크 좌표로 변환
    /// </summary>
    public Vector3 LocalToNetworkPosition(Vector3 localPosition)
    {
        if (CurrentAnchor != null)
            return CurrentAnchor.InverseTransformPoint(localPosition);
        return localPosition;
    }

    /// <summary>
    /// 네트워크 회전을 로컬 회전으로 변환
    /// </summary>
    public Quaternion NetworkToLocalRotation(Quaternion networkRotation)
    {
        if (CurrentAnchor != null)
            return CurrentAnchor.rotation * networkRotation;
        return networkRotation;
    }

    /// <summary>
    /// 로컬 회전을 네트워크 회전으로 변환
    /// </summary>
    public Quaternion LocalToNetworkRotation(Quaternion localRotation)
    {
        if (CurrentAnchor != null)
            return Quaternion.Inverse(CurrentAnchor.rotation) * localRotation;
        return localRotation;
    }
}
