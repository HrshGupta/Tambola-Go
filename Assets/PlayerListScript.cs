using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerListScript : MonoBehaviourPunCallbacks
{
    [SerializeField] Text totalPrizeMoney;
    [SerializeField] Transform content;
    [SerializeField] PlayerListing playerListing;
    List<PlayerListing> listings = new List<PlayerListing>();

    void Start()
    {
        GetCurrentRoomPlayers();
    }

    void GetCurrentRoomPlayers()
    {
        
        foreach(KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log("PlayerNickName" + playerInfo.Value.NickName);
            AddPlayerListing(playerInfo.Value);
        }
    }

    void AddPlayerListing(Player player)
    {
        int playerCount = PhotonNetwork.PlayerList.Length;
        
        switch(SceneManager.GetActiveScene().name)
        {
            case "Lazzy":
                PlayerPrefs.SetFloat("TotalInvestment", playerCount * 50);
                PlayerPrefs.SetFloat("TotalPrize", PlayerPrefs.GetFloat("TotalInvestment") * 0.8f);
                break;
            
            case "Quick":
                PlayerPrefs.SetFloat("TotalInvestment", playerCount * 50);
                PlayerPrefs.SetFloat("TotalPrize", PlayerPrefs.GetFloat("TotalInvestment") * 0.8f);
                break;
            
            case "Rapid":
                PlayerPrefs.SetFloat("TotalInvestment", playerCount * 100);
                PlayerPrefs.SetFloat("TotalPrize", PlayerPrefs.GetFloat("TotalInvestment") * 0.8f);
                break;

            case "Tournament1":
                PlayerPrefs.SetFloat("TotalInvestment", playerCount * 100);
                PlayerPrefs.SetFloat("TotalPrize", PlayerPrefs.GetFloat("TotalInvestment") * 0.875f);
                break;
        }
        
        //totalPrizeMoney.text = "Prize " + PlayerPrefs.GetFloat("TotalPrize").ToString("0");
        
        PlayerListing listing = Instantiate(playerListing, content);
        if(listing != null)
        {
            listing.SetPlayerInfo(player);
            listings.Add(listing);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerListing(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = listings.FindIndex(x => x.Player == otherPlayer);
        if(index != -1)
        {
            Destroy(listings[index].gameObject);
            listings.RemoveAt(index);
        }
    }
}
