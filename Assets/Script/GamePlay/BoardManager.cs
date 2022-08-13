using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Animator panditAnimation;
    [SerializeField] GameObject disconnectedPopUp;
    [SerializeField] Text disconnectedMsg;
    [SerializeField] PlayerBoard _PlayerBoard;
    [SerializeField] Text dealWaitTime;
    [SerializeField] Text dealNumber;
    [SerializeField] List<int> allNumbers = new List<int>(90);
    [SerializeField] float duration = 5f;
    float dealTimer;
    int randomNumber;
    public bool lazzy;
    public bool startGame;

    [Header("All number Clips")]

    [SerializeField] AudioSource numberVoiceSource;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip[] allAudioClips;

    public bool playPanditAnim = false;
    public static BoardManager instance;

    private void Start()
    {

        if (instance == null)
            instance = this;

        disconnectedPopUp.SetActive(false);
        musicSource.Stop();
        startGame = false;
        for(int i = 1; i < 91; i++)
            allNumbers.Add(i);

        //if(SceneManager.GetActiveScene().name == "Rapid" || SceneManager.GetActiveScene().name == "Tournament1" || SceneManager.GetActiveScene().name == "Tournament2" || SceneManager.GetActiveScene().name == "Tournament3")
        //    duration = duration / 1.5f;

        if(SceneManager.GetActiveScene().name == "Lazzy")
            lazzy = true;
        else
            lazzy = false;

        dealTimer = duration;
    }

    void Update()
    {
        //Debug.Log("Player in lobby " + PhotonNetwork.PlayerList.Length);

        if (PhotonNetwork.IsMasterClient && startGame && SceneManager.GetActiveScene().name != "Practice")
        {
            
            photonView.RPC("DealTimerForOthers", RpcTarget.Others, dealWaitTime.text.ToString(), randomNumber);
            DealTimer();
        }

        if(SceneManager.GetActiveScene().name == "Practice" && startGame)
        {
            DealTimer();
        }

        
        if (dealNumber.text.ToString() != randomNumber.ToString())
        {
            if(randomNumber > 0)
            {
                numberVoiceSource.clip = allAudioClips[randomNumber - 1];
                //if(_PlayerBoard.isSoundOn)
                if(PlayerPrefs.GetInt("NumberAudio", 0) == 0)
                    numberVoiceSource.Play();
            }
            // panditAnimation.enabled = false;
            dealNumber.gameObject.SetActive(true);
            dealNumber.text = randomNumber.ToString();
            
            // if(lazzy)
            //if(SceneManager.GetActiveScene().name != "Theme Games")
            //    CheckPlayerNumber(randomNumber);
            
            CheckBoardNumber(randomNumber);
        }
        //else
        //    PlayerBoard.instance.AutoCheck();

        if (Input.GetKeyDown(KeyCode.Space) || playPanditAnim)
        {
            panditAnimation.Play("GirlAnim", 0, 0f);
            playPanditAnim = false;
        }

    }

    public void StopNumberAudio()
    {
        if(PlayerPrefs.GetInt("NumberAudio" , 0) == 0)
        {
            PlayerPrefs.SetInt("NumberAudio", 1);
        }
        else if(PlayerPrefs.GetInt("NumberAudio", 0) == 1)
        {
            PlayerPrefs.SetInt("NumberAudio", 0);
        }
        
    }

    [PunRPC]
    public void StopForOthers()
    {
        startGame = false;
        Invoke("Result", 3);
        // _PlayerBoard.Result(NetworkScript.instance.roomName, true);
    }

    public void StopGame()
    {
        PlayerPrefs.DeleteKey("NumberAudio");
        Debug.Log("game ended");
        photonView.RPC("StopForOthers", RpcTarget.Others);
        startGame = false;
        Invoke("Result", 3);
    }

    public void Result()
    {
        _PlayerBoard.Result(NetworkScript.instance.roomName, true);
    }
    
    [PunRPC]
    void DealTimerForOthers(string message, int dealNumber)
    {
        dealWaitTime.text = message;
        randomNumber = dealNumber;
    }

    void DealTimer()
    {

        
        dealWaitTime.text = dealTimer.ToString("0");

        if (dealTimer > 0)
        {
            dealTimer -= Time.deltaTime;
        }
        else
        {
            photonView.RPC("WaitForNextDealForAll", RpcTarget.Others);
            WaitForNextDealForAll();
            
            StartCoroutine(WaitToPrepareNextDeal());
        }
    }

    [PunRPC]
    void WaitForNextDealForAll()
    {
        dealWaitTime.text = "Preparing for Next Deal";
        dealNumber.gameObject.SetActive(false);
    }

    IEnumerator WaitToPrepareNextDeal()
    {
        yield return new WaitForSeconds(4);
        

        if (dealTimer < 0)
        {
            
            dealTimer = duration;
            
            if (allNumbers.Count > 0)
            {
                //playPanditAnim = false;
                
                int index = Random.Range(0, allNumbers.Count);
                randomNumber = allNumbers[index];
                photonView.RPC("RemoveNumberFromList", RpcTarget.Others, randomNumber);
                RemoveNumberFromList(randomNumber);
                //panditAnimation.StopPlayback();
                // allNumbers.Remove(randomNumber);

            }

            yield return new WaitForSeconds(5);
            Debug.Log("$$$$$ Play set");
            //playPanditAnim = true;
            photonView.RPC("SetAnimation", RpcTarget.All);
            
        }
    }

    [PunRPC]
    void SetAnimation()
    {
        playPanditAnim = true;
    }


    [PunRPC]
    void RemoveNumberFromList(int number)
    {
        allNumbers.Remove(number);
        if(allNumbers.Count == 0)
        {
            //photonView.RPC("StopGame", RpcTarget.All);
            StopGame();
            
            //PlayerBoard.instance.Result(NetworkScript.instance.roomName , true);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {

        disconnectedMsg.text = "Disconnected  due  to." + "\n" + cause.ToString();
        disconnectedPopUp.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        //disconnectedMsg.text = "Disconnected  from  Server.";
        //disconnectedPopUp.SetActive(true);
    }

    public override void OnLeftLobby()
    {
        //disconnectedMsg.text = "Disconnected  from  Server.";
        //disconnectedPopUp.SetActive(true);
    }

    public void OnClickDisconnectOk()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Login");
    }


    void CheckPlayerNumber(int nNumber)
    {
        for (int n = 0; n < Test.instance.numberOfTicket; n++)
        {
            if(_PlayerBoard.allTickets[n].row1[0] != null)
            {
                foreach (Transform Ticket in _PlayerBoard.allTickets[n].row1)
                {
                    if(Ticket.name != "EmptyCell(Clone)")
                    {
                        if (Ticket.GetChild(0).GetComponent<Text>().text == nNumber.ToString())
                        { 
                            Ticket.GetComponent<Image>().enabled = true;
                        }
                    }
                }
                foreach (Transform Ticket in _PlayerBoard.allTickets[n].row2)
                {
                    if(Ticket.name != "EmptyCell(Clone)")
                    {
                        if (Ticket.GetChild(0).GetComponent<Text>().text == nNumber.ToString())
                        { 
                            Ticket.GetComponent<Image>().enabled = true;
                        }
                    }
                }
                foreach (Transform Ticket in _PlayerBoard.allTickets[n].row3)
                {
                    if(Ticket.name != "EmptyCell(Clone)")
                    {
                        if (Ticket.GetChild(0).GetComponent<Text>().text == nNumber.ToString())
                        { 
                            Ticket.GetComponent<Image>().enabled = true;
                        }
                    }
                }
            }
        }
    }

    void CheckBoardNumber(int nNumber)
    {
        foreach(Transform item in _PlayerBoard.boardContainerList)
        {
            if(item.GetChild(1).GetComponent<Text>().text == nNumber.ToString())
            {
                item.GetChild(0).GetComponent<Image>().color = Color.red;
            }
        }
    }

    public void CheckNumberManually(Transform b)
    {
        int n = int.Parse(b.transform.GetChild(0).GetComponent<Text>().text);
        if(!allNumbers.Contains(n))
        {
            b.GetComponent<Image>().enabled = true;
        }
    }

    public void OnApplicationFocus(bool focus)
    {
        if(!focus)
        {
            PhotonNetwork.KeepAliveInBackground = 120;
        }
    }

}
