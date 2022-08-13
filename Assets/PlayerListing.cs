using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListing : MonoBehaviour
{
    [SerializeField] Text nameText;

    public Player Player { get; private set; }

    public void SetPlayerInfo(Player player)
    {
        Player = player;
        //PhotonNetwork.NickName = PlayerPrefs.GetString("UserName") + "_" + PlayerPrefs.GetString("UserID");
        nameText.text = player.NickName;

    }
}
