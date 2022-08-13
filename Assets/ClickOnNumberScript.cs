using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickOnNumberScript : MonoBehaviour
{
    [SerializeField] Transform randomNumber;

    void Start()
    {
        Button b = GetComponent<Button>();
        b.onClick.AddListener(delegate() {OnClickTicketNumber(transform); } );
        randomNumber = GameObject.Find("Canvas/Pandit/DealNumber").transform;
    }

    public void OnClickTicketNumber(Transform button)
    {
        //if(button.GetChild(0).GetComponent<Text>().text == randomNumber.GetComponent<Text>().text)
        //{
        //    button.GetComponent<Image>().enabled = true;
        //}

        BoardManager.instance.CheckNumberManually(button);
    }
}
