using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class NetworkScriptTest : MonoBehaviourPunCallbacks
{
    [SerializeField] string gameName;
    [SerializeField] Text waittingText;
    [SerializeField] int totalPlayerInRoom;
    public int minPlayers;
    public string roomName;
    public int maxPlayers;
    public string gameType;
    [SerializeField] List<string> roomsListName = new List<string>();
    [SerializeField] List<RoomInfo> roomsList = new List<RoomInfo>();
    [SerializeField] GameObject waittingPanel;

    void Start()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.NickName = PlayerPrefs.GetString("UserName") + "\n" + PlayerPrefs.GetString("UserID");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }

    public void EnterGame(string sceneName)
    {
        gameName = sceneName;


        if (roomsList.Count != 0)
        {
            foreach (RoomInfo room in roomsList)
            {
                if (room.IsOpen && room.Name.Split('.')[0] == gameName)
                {
                    PhotonNetwork.JoinRoom(room.Name);
                    return;
                }
            }
        }

        CreateRoom(gameName);
    }

    public void JoinPrivateRoom(string partyName, string password)
    {
        if (roomsList.Count != 0)
        {
            foreach (RoomInfo room in roomsList)
            {
                if (room.IsOpen && room.Name.Split('.')[0] == gameType && room.Name.Split('.')[1] == partyName && room.Name.Split('.')[2] == password)
                {
                    PhotonNetwork.JoinRoom(room.Name);
                    return;
                }
            }
        }
    }

    public void CreatePrivateRoom(string nameOfRoom, string passwordOfRoom)
    {
        roomName = gameType + "." + nameOfRoom + "." + passwordOfRoom;

        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)maxPlayers };

        PhotonNetwork.CreateRoom(roomName, roomOps, TypedLobby.Default);
    }

    void CreateRoom(string game)
    {
        roomName = game + "." + Random.Range(0, 10000);

        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)maxPlayers };

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOps, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        totalPlayerInRoom = PhotonNetwork.PlayerList.Length;
        Debug.Log("Room created");
        waittingText.text = "Waiting For " + (minPlayers - PhotonNetwork.PlayerList.Length).ToString() + " Players\nRoom Name:" + PhotonNetwork.CurrentRoom.Name;
        waittingPanel.SetActive(true);
        if (gameType == "Public")
        {
            if (totalPlayerInRoom >= minPlayers)
                SceneManager.LoadScene(gameName);
        }

        else
        {
            if (totalPlayerInRoom >= minPlayers)
            {
                PlayerPrefs.SetInt("UserTicketsInGame", 6);
                SceneManager.LoadScene(gameType);
            }
        }
    }

    public override void OnJoinedRoom()
    {
        totalPlayerInRoom = PhotonNetwork.PlayerList.Length;
        waittingText.text = "Waiting For " + (minPlayers - PhotonNetwork.PlayerList.Length).ToString() + " Players\nRoom Name:" + PhotonNetwork.CurrentRoom.Name;
        waittingPanel.SetActive(true);
        if (gameType == "Public")
        {
            if (totalPlayerInRoom >= minPlayers)
                SceneManager.LoadScene(gameName);
        }

        else
        {
            if (totalPlayerInRoom >= minPlayers)
            {
                PlayerPrefs.SetInt("UserTicketsInGame", 6);
                SceneManager.LoadScene(gameType);
            }
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CreateRoom(gameName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        totalPlayerInRoom = PhotonNetwork.PlayerList.Length;
        waittingText.text = "Waiting For " + (minPlayers - PhotonNetwork.PlayerList.Length).ToString() + " Players\nRoom Name:" + PhotonNetwork.CurrentRoom.Name;
        waittingPanel.SetActive(true);

        if (gameType == "Public")
        {
            if (totalPlayerInRoom >= minPlayers)
                SceneManager.LoadScene(gameName);
        }

        else
        {
            if (totalPlayerInRoom >= minPlayers)
            {
                PlayerPrefs.SetInt("UserTicketsInGame", 6);
                SceneManager.LoadScene(gameType);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        totalPlayerInRoom = PhotonNetwork.PlayerList.Length;
        waittingText.text = "Waiting For " + (minPlayers - PhotonNetwork.PlayerList.Length).ToString() + " Players\nRoom Name:" + PhotonNetwork.CurrentRoom.Name;
        waittingPanel.SetActive(true);
        if (gameType == "Public")
        {
            if (totalPlayerInRoom >= minPlayers)
                SceneManager.LoadScene(gameName);
        }

        else
        {
            if (totalPlayerInRoom >= minPlayers)
            {
                PlayerPrefs.SetInt("UserTicketsInGame", 6);
                SceneManager.LoadScene(gameType);
            }
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        Debug.Log("Room List Run" + roomList.Count);

        roomsListName.Clear();
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                roomsList.Remove(room);
            }
            else
            {
                roomsList.Add(room);
                roomsListName.Add(room.Name);
            }
        }

    }

    /*public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List Run" + roomList.Count);
       
        roomsListName.Clear();
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                roomsList.Remove(room);
            }
            else
            {
                roomsList.Add(room);
                roomsListName.Add(room.Name);
            }
        }
    }*/

    public void OnClickWaittingBackButton()
    {
        PhotonNetwork.LeaveRoom();
    }


    public void BotGame()
    {
        SceneManager.LoadScene("Practice");
    }

}
