using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PracticeMatch : MonoBehaviour
{

    [SerializeField] Animator panditAnimation;
    [SerializeField] GameObject disconnectedPopUp;
    [SerializeField] Text disconnectedMsg;
    [SerializeField] PlayerBoard _PlayerBoard;
    [SerializeField] Text dealWaitTime;
    [SerializeField] Text dealNumber;
    [SerializeField] List<int> allNumbers = new List<int>(90);
    [SerializeField] float duration = 5f;
    float dealTimer;
    int randomNumber;
    public bool lazzy;
    public bool startGame;

    [Header("All number Clips")]

    [SerializeField] AudioSource numberVoiceSource;
    [SerializeField] AudioClip[] allAudioClips;




    private void Start()
    {
        //disconnectedPopUp.SetActive(false);

        startGame = false;
        for (int i = 1; i < 91; i++)
            allNumbers.Add(i);

        if (SceneManager.GetActiveScene().name == "Rapid" || SceneManager.GetActiveScene().name == "Tournament1" || SceneManager.GetActiveScene().name == "Tournament2" || SceneManager.GetActiveScene().name == "Tournament3")
            duration = duration / 1.5f;

        if (SceneManager.GetActiveScene().name == "Lazzy" || SceneManager.GetActiveScene().name == "Practice")
            lazzy = true;
        else
            lazzy = false;

        dealTimer = duration;
    }

    void Update()
    {

        if (SceneManager.GetActiveScene().name == "Practice" && startGame)
        {
            Debug.Log("Dealing");
            DealTimer();
        }


        if (dealNumber.text.ToString() != randomNumber.ToString())
        {
            if (randomNumber > 0)
            {
                //numberVoiceSource.clip = allAudioClips[randomNumber - 1];
                //numberVoiceSource.Play();
            }
            // panditAnimation.enabled = false;
            dealNumber.gameObject.SetActive(true);
            dealNumber.text = randomNumber.ToString();
            PlayerBoard.instance.AutoCheck();
            // if(lazzy)
            if (SceneManager.GetActiveScene().name != "Theme Games")
                CheckPlayerNumber(randomNumber);

            CheckBoardNumber(randomNumber);
        }

        DealTimerForOthers(dealWaitTime.text.ToString(), randomNumber);
    }

    void DealTimerForOthers(string message, int dealNumber)
    {
        dealWaitTime.text = message;
        randomNumber = dealNumber;
    }

    void DealTimer()
    {


        dealWaitTime.text = dealTimer.ToString("0");

        if (dealTimer > 0)
        {

            dealTimer -= Time.deltaTime;
        }
        else
        {
            WaitForNextDealForAll();
            StartCoroutine(WaitToPrepareNextDeal());
        }

    }

    void WaitForNextDealForAll()
    {
        dealWaitTime.text = "Preparing for Next Deal";
        panditAnimation.StopPlayback();
        //panditAnimation.enabled = true;
        panditAnimation.Play("GirlAnim");
        dealNumber.gameObject.SetActive(false);
        //PlayerBoard.instance.AutoCheck();
    }

    IEnumerator WaitToPrepareNextDeal()
    {
        yield return new WaitForSeconds(4f);

        if (dealTimer < 0)
        {
            dealTimer = duration;
            if (allNumbers.Count > 0)
            {
                int index = Random.Range(0, allNumbers.Count);
                randomNumber = allNumbers[index];
                RemoveNumberFromList(randomNumber);
                // allNumbers.Remove(randomNumber);
            }
        }
    }

    void RemoveNumberFromList(int number)
    {
        allNumbers.Remove(number);
    }



    void CheckPlayerNumber(int nNumber)
    {
        for (int n = 0; n < PlayerPrefs.GetInt("UserTicketsInGame"); n++)
        {
            if (_PlayerBoard.allTickets[n].row1[0] != null)
            {
                foreach (Transform Ticket in _PlayerBoard.allTickets[n].row1)
                {
                    if (Ticket.name != "EmptyCell(Clone)")
                    {
                        if (Ticket.GetChild(0).GetComponent<Text>().text == nNumber.ToString())
                        {
                            Ticket.GetComponent<Image>().enabled = true;
                        }
                    }
                }
                foreach (Transform Ticket in _PlayerBoard.allTickets[n].row2)
                {
                    if (Ticket.name != "EmptyCell(Clone)")
                    {
                        if (Ticket.GetChild(0).GetComponent<Text>().text == nNumber.ToString())
                        {
                            Ticket.GetComponent<Image>().enabled = true;
                        }
                    }
                }
                foreach (Transform Ticket in _PlayerBoard.allTickets[n].row3)
                {
                    if (Ticket.name != "EmptyCell(Clone)")
                    {
                        if (Ticket.GetChild(0).GetComponent<Text>().text == nNumber.ToString())
                        {
                            Ticket.GetComponent<Image>().enabled = true;
                        }
                    }
                }
            }
        }
    }

    void CheckBoardNumber(int nNumber)
    {
        foreach (Transform item in _PlayerBoard.boardContainerList)
        {
            if (item.GetChild(1).GetComponent<Text>().text == nNumber.ToString())
            {
                item.GetChild(0).GetComponent<Image>().color = Color.red;
            }
        }
    }
}
