using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CreatePrivateRoom : MonoBehaviour
{
    [SerializeField] InputField roomName;
    [SerializeField] InputField roomPassword;
    [SerializeField] InputField maxPlayer;
    [SerializeField] InputField maxTickets;
    [SerializeField] InputField startingTicketPrice;
    [SerializeField] Toggle autoGenerateNameAndPassword;
    [SerializeField] Toggle autoMode;
    [SerializeField] NetworkScript networkScript;
    [SerializeField] GameObject claimPanel;
    [SerializeField] GameObject claimContainer;
    [SerializeField] List<string> claimList = new List<string>();

    void Start()
    {
        claimPanel.SetActive(false);
    }
    
    public void OnClickCreateButton()
    {
        

        if(maxPlayer.text.ToString() != "")
        {
            networkScript.maxPlayers = Int32.Parse(maxPlayer.text.ToString());
        }
        else
        {
            networkScript.maxPlayers = 50;
        }

        networkScript.CreatePrivateRoom(roomName.text.ToString(), roomPassword.text.ToString());
        Test.instance.numberOfTicket = int.Parse(maxTickets.text);
    }

    public void OnCliclGameMode()
    {
        
        claimPanel.SetActive(true);
        if(Test.instance.currentGameName.Contains("Six Slip Games"))
        {
            Debug.Log("Setting for six slip" + claimContainer.transform.childCount);
            for (int i = claimContainer.transform.childCount - 1; i > claimContainer.transform.childCount - 7; i--)
            {
                Debug.Log("Setting for six slip" + claimContainer.transform.childCount);
                claimContainer.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = claimContainer.transform.childCount - 1; i > claimContainer.transform.childCount - 7; i--)
            {
                claimContainer.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void OnCliclGameModeBack()
    {
        claimPanel.SetActive(false);
    }

    public void AddButton(Transform button)
    {
        if(button.GetComponent<Image>().color.g == 1 && button.GetComponent<Image>().color.b == 1)
        {
            button.GetComponent<Image>().color = new Color(1, 0, 0, 1);
            claimList.Add(button.name);
            Test.instance.privateTableClaimLists = claimList;
        }
        else
        {
            button.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            claimList.Remove(button.name);
            Test.instance.privateTableClaimLists = claimList;
        }
    }
}
