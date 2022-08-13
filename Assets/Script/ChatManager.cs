using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using UnityEngine.UI;


public class ChatManager : MonoBehaviour , IChatClientListener
{
    public ChatClient chatClient;
    [SerializeField] InputField message;
    [SerializeField] GameObject mssgPrefab;
    [SerializeField] Transform messagePrefabHolder;
      
    public void Start()
    {
        Application.runInBackground = true;
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, new AuthenticationValues(PhotonNetwork.NickName));
    }


    public void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }
      

    }

    public void SendMessage()
    {
        chatClient.PublishMessage("WORLD_CHAT", message.text);
        message.text = "";
        Debug.Log("message was: " + message.text);
    }


    #region Chat callback Listener
    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
    {
        
    }

    public void OnDisconnected()
    {
       
    }

    public void OnConnected()
    {
        Debug.Log("connected");
        chatClient.Subscribe(new string[] { "WORLD_CHAT" });
        chatClient.SetOnlineStatus(ChatUserStatus.Online);

    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log("state : " + state);
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for(int i =0; i < senders.Length; i++)
        {
            GameObject gb = Instantiate(mssgPrefab, messagePrefabHolder.transform);
            gb.GetComponent<Text>().text = senders[i] + ":" + messages[i];
        }
        //Debug.Log("channel name : " + channelName + " sender 1 " + senders[0] + " message: " + messages[0]);
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        this.chatClient.PublishMessage("", "joined");
    }

    public void OnUnsubscribed(string[] channels)
    {
        
    }

    public void OnUserSubscribed(string channel, string result)
    {
        
    }

    public void OnUserUnsubscribed(string channel , string result)
    {
        
    }



    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log("status was updated " + user + " status " + status + " got a message " + gotMessage + " message : " + message);
    }
    #endregion
}
