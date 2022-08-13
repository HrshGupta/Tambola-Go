using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TournamentResult : MonoBehaviourPunCallbacks
{
    public static TournamentResult instance;

    public Dictionary<int, List<string>> result = new Dictionary<int, List<string>>();
    public int pincipleAmount;
    public Dictionary<int, bool> participant = new Dictionary<int, bool>();
    public List<string> userIds = new List<string>();
    public bool tournamentInitialized = false;
    public int testNumber = 0;
    public bool tournamentStarted = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    public void AddDataToDict(int key , bool value)
    {
        photonView.RPC("BroadCastDict" , RpcTarget.All , key  , value);
    }

    [PunRPC]
    public void BroadCastDict(int key , bool value)
    {
        participant.Add(key, value);
    }

}
