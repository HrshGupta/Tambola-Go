using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ThemeGamesScript : MonoBehaviour
{
    [SerializeField] Animator panditAnimation;
    public GameObject Cell;
    public Transform boardContainer;
    [SerializeField] Text dealWaitTime;
    [SerializeField] Text dealNumber;
    [SerializeField] float duration = 5f;
    [SerializeField] List<int> allNumbers = new List<int>(90);
    [SerializeField]
    public List<int> temp_numbers = new List<int>();
    public List<Transform> boardContainerList = new List<Transform>();

    [SerializeField] GameObject quitPanel;

    [SerializeField] AudioSource clickAudio;
    [SerializeField] AudioSource musicAudio;

    [SerializeField] AudioSource numberVoiceSource;
    [SerializeField] AudioClip[] allAudioClips;

    float dealTimer;
    int randomNumber;

    void Start()
    {
        if(PlayerPrefs.GetInt("Music") == 0)
            musicAudio.mute = false;
            
        if(PlayerPrefs.GetInt("Music") == 1)
            musicAudio.mute = true;
        
        if(PlayerPrefs.GetInt("Sound") == 0)
            clickAudio.mute = false;

        if(PlayerPrefs.GetInt("Sound") == 1)
            clickAudio.mute = true;

        CreateBoardCell();

        for(int i = 1; i < 91; i++)
            allNumbers.Add(i);

        dealTimer = duration;

        quitPanel.SetActive(false);
    }

    void Update()
    {
        DealTimer();

        if(dealNumber.text.ToString() != randomNumber.ToString())
        {
            if(randomNumber > 0)
            {
                numberVoiceSource.clip = allAudioClips[randomNumber - 1];
                numberVoiceSource.Play();
            }

            dealNumber.gameObject.SetActive(true);
            dealNumber.text = randomNumber.ToString();
            
            CheckBoardNumber(randomNumber);
        }
    }

    void DealTimer()
    {
        dealWaitTime.text = dealTimer.ToString("0");

        if(dealTimer > 0)
            dealTimer -= Time.deltaTime;
        else
        {
            WaitForNextDealForAll();
            StartCoroutine(WaitToPrepareNextDeal());
        }
    }

    void WaitForNextDealForAll()
    {
        dealWaitTime.text = "Preparing for Next Deal";
        // panditAnimation.enabled = true;
        panditAnimation.Play("DealerAnimation");
        dealNumber.gameObject.SetActive(false);
    }

    IEnumerator WaitToPrepareNextDeal()
    {
        yield return new WaitForSeconds(4f);

        if(dealTimer < 0)
        {
            dealTimer = duration;
            if(allNumbers.Count > 0)
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

    public void CreateBoardCell()
    {

        for (int i = 1; i < 91; i++)
        {
            Debug.Log("Cell Created" + i);
            GameObject cellObject = Instantiate(Cell.gameObject);
            cellObject.transform.SetParent(boardContainer);
            cellObject.transform.GetChild(1).GetComponent<Text>().text = i.ToString();
            boardContainerList.Add(cellObject.transform);
            temp_numbers.Add(i);
        }

    }

    void CheckBoardNumber(int nNumber)
    {
        foreach(Transform item in boardContainerList)
        {
            if(item.GetChild(1).GetComponent<Text>().text == nNumber.ToString())
            {
                item.GetChild(0).GetComponent<Image>().color = Color.red;
            }
        }
    }

    public void OnClickQuitButton()
    {
        clickAudio.Play();
        quitPanel.SetActive(true);
    }

    public void OnClickYesQuitButton()
    {
        clickAudio.Play();
        SceneManager.LoadScene("Login");
    }

    public void OnClickNoQuitButton()
    {
        clickAudio.Play();
        quitPanel.SetActive(false);
    }
}
