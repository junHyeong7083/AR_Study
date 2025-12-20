using UnityEngine;
using Photon.Pun;

/// <summary>
/// VR 방장이 SharedObject를 생성하는 스크립트
/// AR은 마커 인식으로 생성, VR은 이 스크립트로 생성
/// </summary>
public class SharedObjectSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    [SerializeField] private string sharedObjectPrefabName = "DogPolyart";
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 0, 2f);  // 카메라 앞 2m

    [Header("Auto Spawn (VR Only)")]
    [SerializeField] private bool autoSpawnInVR = true;
    [SerializeField] private float spawnDelay = 1f;

    private bool hasSpawned = false;

    private void Start()
    {
        // VR 모드에서만 자동 생성
        if (autoSpawnInVR && XRPlatformManager.Instance != null && XRPlatformManager.Instance.IsVR)
        {
            Invoke(nameof(TrySpawnSharedObject), spawnDelay);
        }
    }


    /// <summary>
    /// SharedObject 생성 시도
    /// </summary>
    public void TrySpawnSharedObject()
    {
        // 이미 생성했으면 스킵
        if (hasSpawned)
        {
            Debug.Log("[SharedObjectSpawner] 이미 생성됨");
            return;
        }

        // 방장만 생성
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[SharedObjectSpawner] 방장이 아니므로 생성하지 않음");
            return;
        }

        SpawnSharedObject();
    }

    /// <summary>
    /// SharedObject 생성
    /// </summary>
    public void SpawnSharedObject()
    {
        Debug.Log($"[SharedObjectSpawner] SharedObject 생성: {sharedObjectPrefabName}");

        GameObject obj = PhotonNetwork.Instantiate(
            sharedObjectPrefabName,
            spawnPosition,
            Quaternion.identity
        );

        hasSpawned = true;
        Debug.Log($"[SharedObjectSpawner] 생성 완료: {obj.name}");
    }

    /// <summary>
    /// 방장이 바뀌면 호출됨
    /// </summary>
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"[SharedObjectSpawner] 방장 변경: {newMasterClient.NickName}");
    }
}
