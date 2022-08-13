using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameStartScript : MonoBehaviour
{
    [SerializeField] NetworkScript networkScript;
    [SerializeField] InputField ticketYouCanTake;
    [SerializeField] GameObject errorMessage;
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] Transform enter;
    [SerializeField] AudioSource clickAudio;
   
    public void OnInputFieldChaged()
    {
        clickAudio.Play();
        if(Int32.Parse(ticketYouCanTake.text.ToString()) > 6)
            ticketYouCanTake.text = "6";
        
        if(Int32.Parse(ticketYouCanTake.text.ToString()) < 1)
            ticketYouCanTake.text = "0";
    }

    public void OnClickEnter()
    {
        clickAudio.Play();
        if(!PhotonNetwork.IsConnected)
            return;

        

        switch(Test.instance.currentGameName)
        {
            case "Tournament1":
                Test.instance.numberOfTicket = 2;
                if(PlayerPrefs.GetInt("UserCoin") >= 100)
                {
                    Reward(100, int.Parse(PlayerPrefs.GetString("UserID")), "debit");
                    PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") - 100);
                    Tournament(Test.instance.currentGameName);
                }
                else
                {
                        errorMessage.GetComponent<Text>().text = "Not Enough Money";
                        errorMessage.SetActive(true);
                        Invoke("OffErrorMessage", 2);
                }

                
                break;
            
            case "Lazzy":
                Reward(Test.instance.numberOfTicket * 10, int.Parse(PlayerPrefs.GetString("UserID")), "debit");
                PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") - Test.instance.numberOfTicket * 10);
                Lazzy(Test.instance.currentGameName);
                break;
            
            case "Rapid":
                Reward(Test.instance.numberOfTicket * 10, int.Parse(PlayerPrefs.GetString("UserID")), "debit");
                PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") - Test.instance.numberOfTicket * 10);
                Default(Test.instance.currentGameName);
                break;
            
            case "Normal":
                Reward(Test.instance.numberOfTicket * 10, int.Parse(PlayerPrefs.GetString("UserID")), "debit");
                PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") - Test.instance.numberOfTicket * 10);
                Default(Test.instance.currentGameName);
                break;
            
            case "Quick":
                Reward(Test.instance.numberOfTicket * 10, int.Parse(PlayerPrefs.GetString("UserID")), "debit");
                PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") - Test.instance.numberOfTicket * 10);
                Default(Test.instance.currentGameName);
                break;
            case "Practice":
                Reward(Test.instance.numberOfTicket * 10, int.Parse(PlayerPrefs.GetString("UserID")), "debit");
                PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") - Test.instance.numberOfTicket * 10);
                Default(Test.instance.currentGameName);
                break;
        }
        
    }

    void Tournament(string sceneName)
    {
        networkScript.gameType = "Public";
        networkScript.maxPlayers = 20;
        networkScript.EnterGame(sceneName);
        
       /* if (Test.instance.numberOfTicket >= 2)
        {
            //PlayerPrefs.SetInt("UserTicketsInGame", 2);
            //PlayerPrefs.SetInt("UserTickets", PlayerPrefs.GetInt("UserTickets") - 2);
            lobbyManager.UserDataSet();
            networkScript.gameType = "Public";
            networkScript.maxPlayers = 20;
            networkScript.EnterGame(sceneName);
        }
        else
        {
            errorMessage.GetComponent<Text>().text = "Not Enough Ticket";
            errorMessage.SetActive(true);
            Invoke("OffErrorMessage", 2);
        }*/
    }

    void Lazzy(string sceneName)
    {
        /*if(ticketYouCanTake.text != null)
        {
            if(PlayerPrefs.GetInt("UserTickets") >= Int32.Parse(ticketYouCanTake.text.ToString()))
            {
                PlayerPrefs.SetInt("UserTicketsInGame", Int32.Parse(ticketYouCanTake.text.ToString()));
                PlayerPrefs.SetInt("UserTickets", PlayerPrefs.GetInt("UserTickets") - PlayerPrefs.GetInt("UserTicketsInGame"));
             lobbyManager.UserDataSet();
            networkScript.gameType = "Public";
            networkScript.maxPlayers = 50;
            networkScript.EnterGame(sceneName);
            }
            else
            {
                errorMessage.GetComponent<Text>().text = "Not Enough Ticket";
                errorMessage.SetActive(true);
                Invoke("OffErrorMessage", 2);
            }

            
        }
        else
        {
            errorMessage.GetComponent<Text>().text = "Input Field is Empty";
            errorMessage.SetActive(true);
            Invoke("OffErrorMessage", 2);
        }*/

        networkScript.gameType = "Public";
        networkScript.maxPlayers = 50;
        networkScript.EnterGame(sceneName);
    }

    void Default(string sceneName)
    {
        /*if(ticketYouCanTake.text.ToString() != "")
        {
            if(PlayerPrefs.GetInt("UserTickets") >= Int32.Parse(ticketYouCanTake.text.ToString()))
            {
                PlayerPrefs.SetInt("UserTicketsInGame", Int32.Parse(ticketYouCanTake.text.ToString()));
                PlayerPrefs.SetInt("UserTickets", PlayerPrefs.GetInt("UserTickets") - PlayerPrefs.GetInt("UserTicketsInGame"));
                lobbyManager.UserDataSet();
                networkScript.gameType = "Public";
                networkScript.maxPlayers = 50;
                networkScript.EnterGame(sceneName);
            }
            else
            {
                errorMessage.GetComponent<Text>().text = "Not Enough Ticket";
                errorMessage.SetActive(true);
                Invoke("OffErrorMessage", 2);
            }
        }
        else
        {
            errorMessage.GetComponent<Text>().text = "Input Field is Empty";
            errorMessage.SetActive(true);
            Invoke("OffErrorMessage", 2);
        }*/

        
        networkScript.gameType = "Public";
        networkScript.maxPlayers = 50;
        networkScript.EnterGame(sceneName);
    }

    void OffErrorMessage()
    {
        errorMessage.SetActive(false);
    }

    void Reward(int amount, int id, string utype)
    {
        var url = StaticDetatils.baseUrl + "Api/Users/wallet.php";
        Wallet wallet = new Wallet()
        {
            user_id = id,
            chip_amount = amount,
            utype = utype
        };
        Debug.Log("sending reward data " + JsonUtility.ToJson(wallet));
        CommunicateWithServer.instance.SendRequest(wallet, url);
        CommunicateWithServer.instance.gotResponse += ResponseWallet;
    }

    void ResponseWallet(bool para)
    {
        Debug.Log("Wallet Response " + CommunicateWithServer.instance.responseData);

        lobbyManager.UserDataSet();
    }


    #region Add money debugging
    public void AddMoney()
    {
        Reward(200, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
        PlayerPrefs.SetInt("UserCoin", PlayerPrefs.GetInt("UserCoin") + 200);
        lobbyManager.UserDataSet();
    }

    #endregion
}
