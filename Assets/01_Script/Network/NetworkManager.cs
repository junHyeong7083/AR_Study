using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Photon 서버 연결 및 상태 관리
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string gameVersion = "1.0";

    // 이벤트
    public System.Action OnConnectedToServer;
    public System.Action OnJoinedLobbyEvent;
    public System.Action<string> OnConnectionFailed;

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

    private void Start()
    {
        ConnectToServer();
    }

    /// <summary>
    /// Photon 서버에 연결
    /// </summary>
    public void ConnectToServer()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[NetworkManager] 이미 연결됨");
            PhotonNetwork.JoinLobby();
            return;
        }

        Debug.Log("[NetworkManager] 서버 연결 중...");
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("[NetworkManager] 마스터 서버 연결 성공!");
        OnConnectedToServer?.Invoke();

        // 자동으로 로비 입장
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[NetworkManager] 로비 입장 성공!");
        OnJoinedLobbyEvent?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[NetworkManager] 연결 해제: {cause}");
        OnConnectionFailed?.Invoke(cause.ToString());
    }

    #endregion
}
