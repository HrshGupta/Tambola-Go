using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TicketPanelScript : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("Theme Games");
    }
}
