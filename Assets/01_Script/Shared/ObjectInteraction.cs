using UnityEngine;
using Photon.Pun;

/// <summary>
/// 오브젝트 상호작용 (회전, 스케일)
/// 버튼으로만 조작
/// </summary>
public class ObjectInteraction : MonoBehaviourPun
{
    [Header("Rotation Settings")]
    [SerializeField] private float buttonRotationAmount = 15f;  // 버튼 클릭당 회전량

    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private float scaleSpeed = 0.1f;

    #region Button Controls

    /// <summary>
    /// 왼쪽으로 회전 (버튼용)
    /// </summary>
    public void RotateLeft()
    {
        RotateObject(-buttonRotationAmount);
    }

    /// <summary>
    /// 오른쪽으로 회전 (버튼용)
    /// </summary>
    public void RotateRight()
    {
        RotateObject(buttonRotationAmount);
    }

    /// <summary>
    /// 크게 (버튼용)
    /// </summary>
    public void ScaleUp()
    {
        ScaleObject(scaleSpeed);
    }

    /// <summary>
    /// 작게 (버튼용)
    /// </summary>
    public void ScaleDown()
    {
        ScaleObject(-scaleSpeed);
    }

    #endregion

    #region Core Functions

    private void RotateObject(float angle)
    {
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            transform.Rotate(Vector3.up, angle, Space.World);
            photonView.RPC("SyncRotation", RpcTarget.Others, transform.rotation);
        }
    }

    private void ScaleObject(float delta)
    {
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            Vector3 newScale = transform.localScale + Vector3.one * delta;
            newScale = Vector3.Max(newScale, Vector3.one * minScale);
            newScale = Vector3.Min(newScale, Vector3.one * maxScale);
            transform.localScale = newScale;
            photonView.RPC("SyncScale", RpcTarget.Others, transform.localScale);
        }
    }

    [PunRPC]
    private void SyncRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    [PunRPC]
    private void SyncScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    #endregion
}
