using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// AR 상태 디버그용 로거
/// SharedRoomScene에 추가하여 AR 상태 확인
/// </summary>
public class ARDebugLogger : MonoBehaviour
{
    private ARSession arSession;
    private ARCameraManager arCameraManager;

    private void Start()
    {
        arSession = FindFirstObjectByType<ARSession>();
        arCameraManager = FindFirstObjectByType<ARCameraManager>();

        Debug.Log("=== AR DEBUG START ===");
        Debug.Log($"[ARDebug] ARSession 찾음: {arSession != null}");
        Debug.Log($"[ARDebug] ARCameraManager 찾음: {arCameraManager != null}");

        if (arSession != null)
        {
            Debug.Log($"[ARDebug] ARSession.enabled: {arSession.enabled}");
            Debug.Log($"[ARDebug] ARSession.gameObject.activeInHierarchy: {arSession.gameObject.activeInHierarchy}");
        }

        if (arCameraManager != null)
        {
            Debug.Log($"[ARDebug] ARCameraManager.enabled: {arCameraManager.enabled}");
        }

        // 카메라 권한 체크
        Debug.Log($"[ARDebug] 카메라 권한: {AndroidPermissionHandler.HasCameraPermission()}");
    }

    private void Update()
    {
        // 매 5초마다 상태 로그
        if (Time.frameCount % 300 == 0)
        {
            LogARState();
        }
    }

    private void LogARState()
    {
        if (arSession != null)
        {
            Debug.Log($"[ARDebug] ARSession.state: {ARSession.state}");
        }
    }
}
