using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using System.Collections;
using UnityEngine.Networking;


public class NetworkScript : MonoBehaviourPunCallbacks
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
    [SerializeField] GameObject hostStartButton;
    bool startGameForOthers = false;
    public static NetworkScript instance;
    public Text timerToEnterGame;
    public GameObject couldNotStartGameText;
    [SerializeField] InputField roomInputField;
    bool tournamentStartedByHost = false;
    [SerializeField] GameObject fbButton, shareBtn;

    void Start()
    {
        instance = this;
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("UserName") + "\n" + PlayerPrefs.GetString("UserID");
        
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
        Debug.Log("room lists " + roomsList.Count);

        if (Test.instance.currentGameName.ToLower().Contains("tournament") || Test.instance.currentGameName.ToLower().Contains("six slip"))
        {
            fbButton.SetActive(false);
            shareBtn.SetActive(false);
        }
        else
        {
            fbButton.SetActive(true);
            shareBtn.SetActive(true);
        }

        if (roomsList.Count != 0)
        {
            foreach(RoomInfo room in roomsList)
            {
                if(room.IsOpen && room.Name.Split('.')[0] == gameName)
                {
                    roomName = room.Name;
                    PhotonNetwork.JoinRoom(room.Name);
                    Debug.Log(PlayerPrefs.GetString("UserID"));
                    StartCoroutine(CreateRoomRoutine(room.Name, PlayerPrefs.GetString("UserID"), name));
                    return;
                }
            }
        }
        else
            CreateRoom(gameName);
    }

    public void JoinWithRoomID()
    {
        Debug.Log(roomInputField.text);
       // if (gameType == "Public" && !Test.instance.currentGameName.ToLower().Contains("tournament"))
        //{
           
            PhotonNetwork.JoinRoom(roomInputField.text);
        //}
            
    }


  

    public void JoinPrivateRoom(string partyName, string password)
    {
        if(roomsList.Count != 0)
        {
            foreach(RoomInfo room in roomsList)
            {
                if(room.IsOpen && room.Name.Split('.')[0] == gameType && room.Name.Split('.')[1] == partyName && room.Name.Split('.')[2] == password)
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

        RoomOptions roomOps = new RoomOptions() {IsVisible = true, IsOpen = true, MaxPlayers = (byte)maxPlayers};

        StartCoroutine(CreateRoomRoutine(roomName, PlayerPrefs.GetString("UserID"), name));

        PhotonNetwork.CreateRoom(roomName, roomOps, TypedLobby.Default);
    }

    void CreateRoom(string game)
    {
        roomName = game + "." + Random.Range(0, 10000);
        
        RoomOptions roomOps = new RoomOptions() {IsVisible = true, IsOpen = true, MaxPlayers = (byte)maxPlayers};

        StartCoroutine(CreateRoomRoutine(roomName , PlayerPrefs.GetString("UserID") , name));

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOps, TypedLobby.Default);
        Debug.Log("************* Created by host **************");
        Test.instance.isHost = true;
        if(Test.instance.currentGameName.ToLower().Contains("tournament"))
        {
            timerToEnterGame.gameObject.SetActive(true);
            StartCoroutine(Timer(60f));
        }
            

    }

    IEnumerator CreateRoomRoutine(string gameId , string userId , string gameName)
    {
        var url = StaticDetatils.baseUrl + "Api/Game/create.php";
        RoomDetail rd = new RoomDetail()
        {
            game_id = gameId,
            user_id = userId,
            game_name = gameName
        };
        
        
        string data = JsonUtility.ToJson(rd);
        Debug.Log(data);


        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.responseCode);
        }

        
    }


    public override void OnCreatedRoom()
    {
        Debug.Log("current players: " + totalPlayerInRoom + " min players: " + minPlayers + " Gametype: " + gameType);
        totalPlayerInRoom = PhotonNetwork.PlayerList.Length;
        
        Test.instance.roomName = PhotonNetwork.CurrentRoom.Name;
        if(Test.instance.currentGameName == "Practice")
        {
            SceneManager.LoadScene(gameName);
            return;
        }
        waittingText.text = "Waiting For " + (minPlayers - PhotonNetwork.PlayerList.Length).ToString() + " Players\nRoom Name:" + PhotonNetwork.CurrentRoom.Name;
        waittingPanel.SetActive(true);
        
        if (gameType == "Public")
        {
            if(totalPlayerInRoom >= minPlayers)
                SceneManager.LoadScene(gameName);
        }

        else
        {
            if(totalPlayerInRoom >= minPlayers)
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
        if(gameType == "Public")
        {
            //if(totalPlayerInRoom >= minPlayers)
            //    SceneManager.LoadScene(gameName);
        }

        else
        {
            if(totalPlayerInRoom >= minPlayers)
            {
                PlayerPrefs.SetInt("UserTicketsInGame", 6);
                SceneManager.LoadScene(gameType);
            }
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        CreateRoom(gameName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        totalPlayerInRoom = PhotonNetwork.PlayerList.Length;
        if(Test.instance.currentGameName.ToLower().Contains("tournament"))
        {
            TournamentResult.instance.pincipleAmount += 100;
            //TournamentResult.instance.participant.Add(int.Parse(PlayerPrefs.GetString("UserID")), Test.instance.isHost);
            //TournamentResult.instance.AddDataToDict(int.Parse(PlayerPrefs.GetString("UserID")), Test.instance.isHost);
            //TournamentResult.instance.userIds.Add(PlayerPrefs.GetString("UserID"));
        }
        waittingText.text = "Waiting For " + (minPlayers - PhotonNetwork.PlayerList.Length).ToString() + " Players\nRoom Name:" + PhotonNetwork.CurrentRoom.Name;
        waittingPanel.SetActive(true);

        if(gameType == "Public")
        {
            if(totalPlayerInRoom >= minPlayers)
            {
                if(gameName.ToLower().Contains("tournament"))
                {
                    if(Test.instance.isHost)
                    {
                        hostStartButton.SetActive(true);
                        hostStartButton.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GoToGame();
                            tournamentStartedByHost = true;
                            photonView.RPC("GoToGame", RpcTarget.Others);
                        });
                    } 
                }
                else
                    SceneManager.LoadScene(gameName);
            }
        }

        else
        {
            if(totalPlayerInRoom >= minPlayers)
            {
                PlayerPrefs.SetInt("UserTicketsInGame", 6);
                SceneManager.LoadScene(gameType);
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message);
    }

    [PunRPC]
    public void GoToGame()
    {
        SceneManager.LoadScene(gameName);
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        totalPlayerInRoom = PhotonNetwork.PlayerList.Length;
        waittingText.text = "Waiting For " + (minPlayers - PhotonNetwork.PlayerList.Length).ToString() + " Players\nRoom Name:" + PhotonNetwork.CurrentRoom.Name;
        waittingPanel.SetActive(true);
        if(gameType == "Public")
        {
            if(totalPlayerInRoom >= minPlayers)
                SceneManager.LoadScene(gameName);
        }

        else
        {
            if(totalPlayerInRoom >= minPlayers)
            {
                PlayerPrefs.SetInt("UserTicketsInGame", 6);
                SceneManager.LoadScene(gameType);
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List Run");
        roomsListName.Clear();
        foreach(RoomInfo room in roomList)
        {
            if(room.RemovedFromList)
            {
                roomsList.Remove(room);
            }
            else
            {
                roomsList.Add(room);
                roomsListName.Add(room.Name);
            }

            Debug.Log("Room name: " + room.Name);
        }
    }

    public void OnClickWaittingBackButton()
    {
        PhotonNetwork.LeaveRoom();
    }


    public void BotGame()
    {
        SceneManager.LoadScene("Practice");
    }


    public IEnumerator Timer(float t)
    {
        while(true)
        {
            t -= Time.deltaTime;
            string s = t.ToString("F2");
            timerToEnterGame.text = s;
            char[] c = s.ToCharArray();
            if(c[1] == '.')
            {
                c[1] = c[0];
                c[0] = '0';
            }
            timerToEnterGame.text = "00 : " + c[0] + c[1];

            if(t <= 0)
            {
                t = 60;
                if (totalPlayerInRoom < minPlayers)
                {
                    couldNotStartGameText.SetActive(true);
                    couldNotStartGameText.GetComponent<Text>().text = "All players not joined";
                    yield return new WaitForSeconds(3);
                    waittingPanel.SetActive(false);
                    couldNotStartGameText.SetActive(false);
                    couldNotStartGameText.GetComponent<Text>().text = "";
                    PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") + 100);
                }
                else if(!tournamentStartedByHost)
                {
                    couldNotStartGameText.SetActive(true);
                    couldNotStartGameText.GetComponent<Text>().text = "You did not started the game";
                    hostStartButton.SetActive(false);
                    yield return new WaitForSeconds(3);
                    waittingPanel.SetActive(false);
                    couldNotStartGameText.GetComponent<Text>().text = "";
                    couldNotStartGameText.SetActive(false);
                    PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") + 100);
                    photonView.RPC("BroadcastForTournament", RpcTarget.Others, "You did not started the game");
                }


                    break;
            }
            yield return null;
        }

        PhotonNetwork.LeaveRoom();
        timerToEnterGame.gameObject.SetActive(false);

    }

    [PunRPC]
    public void BroadcastForTournament(string mssg)
    {
        StartCoroutine(BroadcastForTournamentRoutine());
    }

    [PunRPC]
   IEnumerator BroadcastForTournamentRoutine()
    {
        couldNotStartGameText.SetActive(true);
        couldNotStartGameText.GetComponent<Text>().text = "Host did not started the game";
        yield return new WaitForSeconds(3);
        waittingPanel.SetActive(false);
        couldNotStartGameText.SetActive(false);
        couldNotStartGameText.GetComponent<Text>().text = "";
        
        PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") + 100);
        PhotonNetwork.LeaveRoom();
        timerToEnterGame.gameObject.SetActive(false);
    }
}



public class RoomDetail
{
    public string game_id;
    public string user_id;
    public string game_name;
}