using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class CountdownDelay : MonoBehaviourPunCallbacks
{   
    [SerializeField] BoardManager boardManager;
    [SerializeField] PlayerBoard playerBoard;
    [SerializeField] PracticeMatch practice;
    [SerializeField] int minPlayer = 3;
    public Text waitTime;
    [SerializeField] float duration = 1f;  
    [SerializeField] float secondsLeft; 
    [SerializeField] bool startTimer;
    private void Start()
    {   
        playerBoard = GameObject.Find("PlayerBoardCreate").GetComponent<PlayerBoard>();
        secondsLeft = duration;
        startTimer = false;
        //if(SceneManager.GetActiveScene().name != "Tournament1")
            startTimer = true;

        if(SceneManager.GetActiveScene().name == "Normal Slip Games" || SceneManager.GetActiveScene().name == "Six Slip Games")
        {
            Debug.Log("Loading private scene........");
            foreach(Transform item in playerBoard.allButtons)
            {
                item.GetComponent<Button>().enabled = false;
                item.gameObject.SetActive(false);
            }


            if (PhotonNetwork.IsMasterClient)
            {

                foreach (string claim in Test.instance.privateTableClaimLists)
                {
                    Debug.Log("Setting claims....");
                    DistributeClaimForNormal(claim);
                    photonView.RPC("DistributeClaimForNormal", RpcTarget.Others, claim);
                }

                //Test.instance.privateTableClaimLists.Clear();
            }
        }

    }

    void Update()
    {
        if(PhotonNetwork.IsMasterClient && startTimer && SceneManager.GetActiveScene().name != "Practice")
        {
            photonView.RPC("SecondExchangeForOthers", RpcTarget.Others, secondsLeft);
            CountDown();
        }

        if(SceneManager.GetActiveScene().name == "Practice")
        {
            CountDown();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {        
        int playerCount = PhotonNetwork.PlayerList.Length;
        
       

        if(playerCount >= minPlayer)
        {
            photonView.RPC("StartTimer", RpcTarget.Others);
            StartTimer();
        }

        if (PhotonNetwork.IsMasterClient)
        {

            foreach (string claim in Test.instance.privateTableClaimLists)
            {
                Debug.Log("Setting claims....");
                DistributeClaimForNormal(claim);
                photonView.RPC("DistributeClaimForNormal", RpcTarget.Others, claim);
            }

            //Test.instance.privateTableClaimLists.Clear();
        }
    }

    [PunRPC]
    void StartTimer()
    {
        startTimer = true;
    }

    [PunRPC]
    void StartGame(string gameMode)
    {
        Debug.Log("Starting game");
        boardManager.startGame = true;
        //boardManager.startGame = true;

        
    }


    void CountDown()
    {   
        if(secondsLeft > 0)
            secondsLeft -= Time.deltaTime;
            
        else
        {
            if(SceneManager.GetActiveScene().name == "Practice")
            {
                StartGame("Lazzy");
                DeactivateGameObject();
                return;
            }
            //Debug.Log("#### Claim List " + NormalSlipGameClaimData.claimList);
            if (SceneManager.GetActiveScene().name == "Normal Slip Games" || SceneManager.GetActiveScene().name == "Six Slip Games")
            {
                photonView.RPC("StartGame", RpcTarget.Others, "six slip");
                StartGame("six slip");
            }
            else
            {
                photonView.RPC("StartGame", RpcTarget.Others, "");
                StartGame("");
            }
              


            PhotonNetwork.CurrentRoom.IsOpen = false;
            photonView.RPC("DeactivateGameObject", RpcTarget.Others);
            DeactivateGameObject();
        }

        waitTime.text = "Wait For Other Players " + secondsLeft.ToString("0");
    }

    [PunRPC]
    void SecondExchangeForOthers(float message)
    {
        waitTime.text = "Wait For Other Players " + message.ToString("0");
    }

    [PunRPC]
    void DeactivateGameObject()
    {
        gameObject.SetActive(false);
    }

    [PunRPC]
    void DistributeClaimForNormal(string claim)
    {
        Debug.Log("setting buttons");
        foreach (Transform button in playerBoard.allButtons)
        {
            Debug.Log("Claim : " + claim + " button: " + button.name);
            if (button.name == claim) 
            {
                button.gameObject.SetActive(true);
                button.GetComponent<Button>().enabled = true;
            }
        }
    }
}

