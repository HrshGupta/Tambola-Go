using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;


public class OnlinePlayerHandler : MonoBehaviourPunCallbacks
{
    public Action<Player> onPlayerLeftRoom;
    public Action<Player> onPlayerJoinedRoom;
    public Action<DisconnectCause> onPlayerDisconnected;
    public static OnlinePlayerHandler instance;

    private void Awake()
    {
        instance = this;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public bool AreAllPlayersPresentInRoom()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("<color=green>Player left room </color> " + otherPlayer.NickName);
        onPlayerLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("<color=green>Player entered room </color> " + newPlayer.NickName);
        onPlayerJoinedRoom?.Invoke(newPlayer);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        onPlayerDisconnected?.Invoke(cause);
    }

    public void Disconnect()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Joined)
            PhotonNetwork.LeaveRoom();

        PhotonNetwork.Disconnect();
    }
}