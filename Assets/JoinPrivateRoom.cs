using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinPrivateRoom : MonoBehaviour
{
    [SerializeField] InputField roomName;
    [SerializeField] InputField roomPassword;
    [SerializeField] NetworkScript networkScript;

    public void OnClickJoinRoomButton()
    {
        if(roomName.text != null && roomPassword.text != null)
        {
            networkScript.JoinPrivateRoom(roomName.text.ToString(), roomPassword.text.ToString());
        }
    }
}
