using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Photon.Pun;

/// <summary>
/// AR 마커 인식 시 네트워크 오브젝트 생성
/// 모든 클라이언트가 공유해서 볼 수 있음
/// </summary>
public class ARMultiTrackedImageController : MonoBehaviourPunCallbacks
{
    public ARTrackedImageManager arTrackedImageManager;

    [Header("Network Prefabs (Resources 폴더에 있어야 함)")]
    public string[] prefabNames;  // Resources 폴더의 프리팹 이름들

    // 로컬에서 생성된 오브젝트 (AR용 - 마커 추적)
    private Dictionary<string, GameObject> spawnObjs = new Dictionary<string, GameObject>();

    // 네트워크로 생성된 SharedObject
    private Dictionary<string, GameObject> networkObjs = new Dictionary<string, GameObject>();

    // 현재 트래킹 중인 마커
    private Dictionary<string, ARTrackedImage> trackedImages = new Dictionary<string, ARTrackedImage>();

    public override void OnEnable()
    {
        base.OnEnable();
        if (arTrackedImageManager != null)
            arTrackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (arTrackedImageManager != null)
            arTrackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added) HandleAddedImage(trackedImage);
        foreach (ARTrackedImage trackedImage in eventArgs.updated) HandleUpdatedImage(trackedImage);
        foreach (var removed in eventArgs.removed) HandleRemovedImage(removed.Value);
    }

    private void HandleAddedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        // 마커 정보 저장
        trackedImages[imageName] = trackedImage;

        // XRPlatformManager에 앵커 설정
        if (XRPlatformManager.Instance != null)
        {
            XRPlatformManager.Instance.SetARMarkerAnchor(trackedImage.transform);
        }

        int prefabIdx = GetPrefabIndexByImageName(imageName);
        if (prefabIdx < 0 || prefabIdx >= prefabNames.Length || string.IsNullOrEmpty(prefabNames[prefabIdx])) return;

        // 이미 생성된 오브젝트가 있으면 스킵
        if (networkObjs.ContainsKey(imageName)) return;

        // 방장만 네트워크 오브젝트 생성
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnNetworkObject(imageName, prefabIdx, trackedImage.transform);
        }
    }

    private void SpawnNetworkObject(string imageName, int prefabIdx, Transform markerTransform)
    {
        Debug.Log($"[ARController] 네트워크 오브젝트 생성: {prefabNames[prefabIdx]}");

        // 네트워크 좌표로 변환 (앵커 기준 상대 좌표)
        Vector3 spawnPos = Vector3.zero;  // 앵커 기준 원점

        GameObject spawnedObj = PhotonNetwork.Instantiate(
            prefabNames[prefabIdx],
            spawnPos,
            Quaternion.identity
        );

        // RPC로 모든 클라이언트에 이미지 이름 전달
        PhotonView pv = spawnedObj.GetComponent<PhotonView>();
        if (pv != null)
        {
            photonView.RPC("RegisterNetworkObject", RpcTarget.AllBuffered, imageName, pv.ViewID);
        }
    }

    [PunRPC]
    private void RegisterNetworkObject(string imageName, int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            networkObjs[imageName] = pv.gameObject;
            Debug.Log($"[ARController] 네트워크 오브젝트 등록: {imageName}");
        }
    }

    private void HandleUpdatedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        trackedImages[imageName] = trackedImage;

        // XRPlatformManager에 앵커 업데이트
        if (XRPlatformManager.Instance != null && XRPlatformManager.Instance.IsAR)
        {
            XRPlatformManager.Instance.SetARMarkerAnchor(trackedImage.transform);
        }

        // 네트워크 오브젝트가 없으면 생성 시도
        if (!networkObjs.ContainsKey(imageName))
        {
            HandleAddedImage(trackedImage);
            return;
        }

        // 네트워크 오브젝트 위치 업데이트 (AR 모드에서만)
        if (networkObjs.TryGetValue(imageName, out GameObject networkObj))
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                networkObj.SetActive(true);
            }
            else if (trackedImage.trackingState == TrackingState.Limited)
            {
                // Limited 상태에서도 보이게 할지 선택
                // networkObj.SetActive(false);
            }
        }
    }

    private void HandleRemovedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        trackedImages.Remove(imageName);

        // 네트워크 오브젝트는 삭제하지 않음 (다른 클라이언트도 보고 있을 수 있음)
        // 필요하면 여기서 비활성화 처리
    }

    private int GetPrefabIndexByImageName(string imageName)
    {
        if (arTrackedImageManager.referenceLibrary != null)
        {
            for (int i = 0; i < arTrackedImageManager.referenceLibrary.count; ++i)
            {
                if (arTrackedImageManager.referenceLibrary[i].name == imageName) return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 특정 이미지의 네트워크 오브젝트 가져오기
    /// </summary>
    public GameObject GetNetworkObject(string imageName)
    {
        networkObjs.TryGetValue(imageName, out GameObject obj);
        return obj;
    }

    /// <summary>
    /// 특정 이미지의 트래킹 정보 가져오기
    /// </summary>
    public ARTrackedImage GetTrackedImage(string imageName)
    {
        trackedImages.TryGetValue(imageName, out ARTrackedImage img);
        return img;
    }
}
