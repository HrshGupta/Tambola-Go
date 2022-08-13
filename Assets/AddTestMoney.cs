using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AddTestMoney : MonoBehaviour
{
    [SerializeField] InputField money;
    [SerializeField] LobbyManager lobbyManager;

    public void OnClickCancle()
    {
        gameObject.SetActive(false);
    }

    public void OnClickAddMoney()
    {
        if(money.text.ToString() != "")
        {
            PlayerPrefs.SetInt("UserCoin", Int32.Parse(money.text.ToString()));
            foreach(Text item in lobbyManager.userCoins)
            {
                item.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }
            lobbyManager.UserDataSet();
        }
        else
        {
            AndroidToastMsg.ShowAndroidToastMessage("Give Input");
        }
    }
}
