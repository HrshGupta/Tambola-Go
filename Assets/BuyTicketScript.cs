using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BuyTicketScript : MonoBehaviour
{
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] InputField numberOfTickets;
    [SerializeField] Text cost;
    [SerializeField] GameObject errorMessage;
    [SerializeField] AudioSource clickAudio;
    [SerializeField] GameStartScript gameStartScripts;
    [SerializeField] GameObject confirmPopup;

    public void OnValueChanged()
    {   
        if(Int32.Parse(numberOfTickets.text.ToString()) < 1)
            numberOfTickets.text = "0";

        cost.text = (Int32.Parse(numberOfTickets.text.ToString()) * 10).ToString();
    }

    public void OnClickBuy()
    {
        clickAudio.Play();
        
        if(PlayerPrefs.GetInt("UserCoin") >= Int32.Parse(cost.text.ToString()))
        {
            
            //PlayerPrefs.SetInt("UserTickets", PlayerPrefs.GetInt("UserTickets") + Int32.Parse(numberOfTickets.text.ToString()));
            Test.instance.numberOfTicket = int.Parse(numberOfTickets.text.ToString());
            lobbyManager.UserDataSet();
            confirmPopup.SetActive(true);
            //gameStartScripts.OnClickEnter();
            //gameObject.SetActive(false);
        }
        else
        {
            errorMessage.GetComponent<Text>().text = "Not Enough Money";
            errorMessage.SetActive(true);
            Invoke("OffErrorMessage", 2);
        }
    }

    void OffErrorMessage()
    {
        errorMessage.SetActive(false);
    }


    public void Confirm()
    {
        gameStartScripts.OnClickEnter();
        gameObject.SetActive(false);
    }

}
