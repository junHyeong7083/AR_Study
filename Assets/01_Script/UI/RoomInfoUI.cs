using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// SharedRoomScene에서 현재 상태 표시
/// </summary>
public class RoomInfoUI : MonoBehaviour
{
    [SerializeField] private Text infoText;

    private void Update()
    {
        if (infoText == null) return;

        string platform = "알 수 없음";
        string roomName = "없음";
        int playerCount = 0;

        // 플랫폼 정보
        if (XRPlatformManager.Instance != null)
        {
            if (XRPlatformManager.Instance.IsAR)
                platform = "AR";
            else if (XRPlatformManager.Instance.IsVR)
                platform = "VR";
        }
        else if (RoomManager.Instance != null)
        {
            platform = RoomManager.Instance.SelectedPlatform.ToString();
        }

        // 방 정보
        if (PhotonNetwork.InRoom)
        {
            roomName = PhotonNetwork.CurrentRoom.Name;
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        }

        infoText.text = $"플랫폼: {platform}\n방 이름: {roomName}\n플레이어: {playerCount}명";
    }
}
