using UnityEngine;

/// <summary>
/// Android 권한 요청 핸들러
/// AR 카메라 사용을 위한 카메라 권한 요청
/// 순수 Android API 직접 호출 방식 (androidx 불필요)
/// </summary>
public class AndroidPermissionHandler : MonoBehaviour
{
    private const string CAMERA_PERMISSION = "android.permission.CAMERA";
    private const int PERMISSION_GRANTED = 0;

    private void Start()
    {
        RequestCameraPermission();
    }

    public void RequestCameraPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // 권한 체크
                int permissionStatus = activity.Call<int>("checkSelfPermission", CAMERA_PERMISSION);

                if (permissionStatus != PERMISSION_GRANTED)
                {
                    Debug.Log("[PermissionHandler] 카메라 권한 요청 중...");
                    activity.Call("requestPermissions", new string[] { CAMERA_PERMISSION }, 0);
                }
                else
                {
                    Debug.Log("[PermissionHandler] 카메라 권한 이미 허용됨");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PermissionHandler] 권한 요청 오류: {e.Message}");
        }
#endif
    }

    public static bool HasCameraPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                int permissionStatus = activity.Call<int>("checkSelfPermission", CAMERA_PERMISSION);
                return permissionStatus == PERMISSION_GRANTED;
            }
        }
        catch
        {
            return false;
        }
#else
        return true;
#endif
    }
}
