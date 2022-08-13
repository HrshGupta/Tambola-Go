using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;

public class allLinesScript : MonoBehaviourPunCallbacks
{
    [SerializeField] PhotonView photonView;
    [SerializeField] Transform[] allLinesButtons;
    [SerializeField] PlayerBoard playerBoard;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void OnClickAllLines(Transform currentButton)
    {
        for(int i = 0; i < allLinesButtons.Length; i++)
        {
            if(currentButton.name == allLinesButtons[i].name)
            {
                Debug.Log((i / 3).ToString("0")); // To Get Ticket index
                Debug.Log(i - ((int)(i/3)) * 3); // To Get Ticket row no.

                if((i - ((int)(i/3)) * 3) == 0)
                {
                    int countEnable = 0;
                    foreach(Transform item in playerBoard.allTickets[(int)(i/3)].row1Numbers)
                    {
                        if(item.GetComponent<Image>().isActiveAndEnabled)
                            countEnable++;
                    }

                    if(countEnable == 5)
                    {
                        currentButton.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                        currentButton.GetComponent<Button>().enabled = false;
                        StartCoroutine(DisableButton(currentButton.name, 0f));
                        playerBoard.lineDone(true, currentButton.name);
                    }
                    else
                    {
                        playerBoard.lineDone(false, currentButton.name);
                    }
                }

                if((i - ((int)(i/3)) * 3) == 1)
                {
                    int countEnable = 0;
                    foreach(Transform item in playerBoard.allTickets[(int)(i/3)].row2Numbers)
                    {
                        if(item.GetComponent<Image>().isActiveAndEnabled)
                            countEnable++;
                    }

                    if(countEnable == 5)
                    {
                        currentButton.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                        currentButton.GetComponent<Button>().enabled = false;
                        StartCoroutine(DisableButton(currentButton.name, 0f));
                        playerBoard.lineDone(true, currentButton.name);
                    }
                    else
                    {
                        playerBoard.lineDone(false, currentButton.name);
                    }
                }

                if((i - ((int)(i/3)) * 3) == 2)
                {
                    int countEnable = 0;
                    foreach(Transform item in playerBoard.allTickets[(int)(i/3)].row3Numbers)
                    {
                        if(item.GetComponent<Image>().isActiveAndEnabled)
                            countEnable++;
                    }

                    if(countEnable == 5)
                    {
                        currentButton.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                        currentButton.GetComponent<Button>().enabled = false;
                        StartCoroutine(DisableButton(currentButton.name, 0f));
                        playerBoard.lineDone(true, currentButton.name);
                    }
                    else
                    {
                        playerBoard.lineDone(false, currentButton.name);
                    }
                }

            }
        }
    }

    IEnumerator DisableButton(string buttonName, float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC("DisableButtonForOthers", RpcTarget.Others, buttonName);
        DisableButtonForOthers(buttonName);
    }

    [PunRPC]
    void DisableButtonForOthers(string buttonName)
    {
        int count = 0;

        foreach(Transform item in allLinesButtons)
        {
            if(item.name == buttonName)
            {
                item.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                item.GetComponent<Button>().enabled = false;
            }

            if(!item.GetComponent<Button>().enabled)
                count++;
        }

        if(count == allLinesButtons.Length)
        {
            playerBoard.all18LineDone();
        }
    }
}
