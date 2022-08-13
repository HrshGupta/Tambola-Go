using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public static Test instance;
    public string currentGameName;
    public string roomName;
    public int numberOfTicket = 1;
    public bool isHost = false;
    public List<string> privateTableClaimLists = new List<string>();
         
    private void Awake()
    {
        //PlayerPrefs.DeleteAll();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(instance);

        //PlayerPrefs.SetInt("UserCoin", 1000);
    }

}
