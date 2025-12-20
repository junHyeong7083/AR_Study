using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class LobbyUI : MonoBehaviour
{
    [Header("Connection Status")]
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Text statusText;

    [Header("Room List")]
    [SerializeField] private Transform roomListContent;
    [SerializeField] private GameObject roomItemPrefab;
    [SerializeField] private Text noRoomText;

    [Header("Create Room")]
    [SerializeField] private InputField roomNameInput;
    [SerializeField] private Button createRoomButton;

    [Header("Platform Selection")]
    [SerializeField] private Toggle arToggle;
    [SerializeField] private Toggle vrToggle;

    private List<GameObject> roomItemList = new List<GameObject>();

    private void Start()
    {
        ShowConnectingPanel();

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnJoinedLobbyEvent += OnJoinedLobby;
            NetworkManager.Instance.OnConnectionFailed += OnConnectionFailed;
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomListUpdated += UpdateRoomList;
            RoomManager.Instance.OnRoomJoinFailed += OnRoomJoinFailed;
        }

        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoomClicked);

        if (arToggle != null)
            arToggle.onValueChanged.AddListener(OnARToggleChanged);

        if (vrToggle != null)
            vrToggle.onValueChanged.AddListener(OnVRToggleChanged);

        if (arToggle != null)
            arToggle.isOn = true;

    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnJoinedLobbyEvent -= OnJoinedLobby;
            NetworkManager.Instance.OnConnectionFailed -= OnConnectionFailed;
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomListUpdated -= UpdateRoomList;
            RoomManager.Instance.OnRoomJoinFailed -= OnRoomJoinFailed;
        }
    }

    private void ShowConnectingPanel()
    {
        if (connectingPanel != null) connectingPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (statusText != null) statusText.text = "서버 연결 중...";
    }

    private void ShowLobbyPanel()
    {
        if (connectingPanel != null) connectingPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (statusText != null) statusText.text = "연결됨";
    }

    private void OnJoinedLobby()
    {
        ShowLobbyPanel();
    }

    private void OnConnectionFailed(string reason)
    {
        if (statusText != null)
            statusText.text = "연결 실패: " + reason;
    }

    private void OnRoomJoinFailed(string reason)
    {
        if (statusText != null)
            statusText.text = "방 참가 실패: " + reason;
    }

    private void UpdateRoomList(List<RoomInfo> rooms)
    {
        foreach (var item in roomItemList)
        {
            Destroy(item);
        }
        roomItemList.Clear();

        if (noRoomText != null)
            noRoomText.gameObject.SetActive(rooms.Count == 0);

        foreach (var room in rooms)
        {
            CreateRoomItem(room);
        }
    }

    private void CreateRoomItem(RoomInfo room)
    {
        if (roomItemPrefab == null || roomListContent == null) return;

        GameObject item = Instantiate(roomItemPrefab, roomListContent);
        roomItemList.Add(item);

        Text roomNameText = item.GetComponentInChildren<Text>();
        if (roomNameText != null)
        {
            roomNameText.text = room.Name + " (" + room.PlayerCount + "/" + room.MaxPlayers + ")";
        }

        Button joinButton = item.GetComponentInChildren<Button>();
        if (joinButton != null)
        {
            string roomName = room.Name;
            joinButton.onClick.AddListener(() => OnJoinRoomClicked(roomName));
        }
    }

    private void OnCreateRoomClicked()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : "";
        RoomManager.Instance?.CreateRoom(roomName);
    }

    private void OnJoinRoomClicked(string roomName)
    {
        RoomManager.Instance?.JoinRoom(roomName);
    }

    private void OnARToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (vrToggle != null) vrToggle.isOn = false;
            RoomManager.Instance?.SetPlatform(RoomManager.PlatformType.AR);
            UpdateCreateButtonState(true);
        }
    }

    private void OnVRToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (arToggle != null) arToggle.isOn = false;
            RoomManager.Instance?.SetPlatform(RoomManager.PlatformType.VR);
            UpdateCreateButtonState(true);  // VR도 방 생성 가능
        }
    }

    private void UpdateCreateButtonState(bool canCreate)
    {
        if (createRoomButton != null)
        {
            createRoomButton.interactable = canCreate;
        }

        if (roomNameInput != null)
        {
            roomNameInput.interactable = canCreate;
        }
    }
}
