using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
/// <summary>
/// 방 생성, 참가, 목록 관리
/// </summary>
public class RoomManager : MonoBehaviourPunCallbacks
{

    public static RoomManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "SharedRoomScene";
    [SerializeField] private int maxPlayersPerRoom = 4;

    // 방 목록 (Dictionary로 누적 관리)
    private Dictionary<string, RoomInfo> roomDict = new Dictionary<string, RoomInfo>();
    public List<RoomInfo> RoomList { get; private set; } = new List<RoomInfo>();

    // 이벤트
    public System.Action<List<RoomInfo>> OnRoomListUpdated;
    public System.Action OnRoomCreated;
    public System.Action OnRoomJoined;
    public System.Action<string> OnRoomJoinFailed;

    // 플랫폼 타입 (AR 또는 VR)
    public enum PlatformType { AR, VR }
    public PlatformType SelectedPlatform { get; private set; } = PlatformType.AR;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 플랫폼 선택 (AR/VR)
    /// </summary>
    public void SetPlatform(PlatformType platform)
    {
        SelectedPlatform = platform;
        Debug.Log($"[RoomManager] 플랫폼 선택: {platform}");
    }

    /// <summary>
    /// 새 방 생성
    /// </summary>
    public void CreateRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room_{Random.Range(1000, 9999)}";
        }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        Debug.Log($"[RoomManager] 방 생성 중: {roomName}");
        PhotonNetwork.CreateRoom(roomName, options);
    }

    /// <summary>
    /// 기존 방 참가
    /// </summary>
    public void JoinRoom(string roomName)
    {
        Debug.Log($"[RoomManager] 방 참가 중: {roomName}");
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// 방 나가기
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    #region Photon Callbacks

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"[RoomManager] 방 목록 업데이트: {roomList.Count}개 변경");

        // 변경된 방만 업데이트 (누적 관리)
        foreach (var room in roomList)
        {
            if (room.RemovedFromList || !room.IsOpen || !room.IsVisible || room.PlayerCount == 0)
            {
                // 삭제된 방이거나 비공개/닫힌 방이면 제거
                roomDict.Remove(room.Name);
            }
            else
            {
                // 새 방이거나 업데이트된 방이면 추가/갱신
                roomDict[room.Name] = room;
            }
        }

        // Dictionary를 List로 변환
        RoomList.Clear();
        RoomList.AddRange(roomDict.Values);

        Debug.Log($"[RoomManager] 현재 총 방 개수: {RoomList.Count}개");
        OnRoomListUpdated?.Invoke(RoomList);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("[RoomManager] 방 생성 성공!");
        OnRoomCreated?.Invoke();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[RoomManager] 방 입장 성공! 방 이름: {PhotonNetwork.CurrentRoom.Name}");
        OnRoomJoined?.Invoke();

        // 플랫폼 정보를 Custom Properties에 저장
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "Platform", SelectedPlatform.ToString() }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 게임 씬으로 이동
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[RoomManager] 방 참가 실패: {message}");
        OnRoomJoinFailed?.Invoke(message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[RoomManager] 방 생성 실패: {message}");
        OnRoomJoinFailed?.Invoke(message);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[RoomManager] 방 퇴장");
        SceneManager.LoadScene("LobbyScene");
    }

    #endregion
}
