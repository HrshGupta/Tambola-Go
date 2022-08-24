using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEngine.Networking;
using System.Reflection;

public class PlayerBoard : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PlayerBoard instance;
    [SerializeField] GameObject wonPopUp;
    [SerializeField] AudioSource clickAudio;
    [SerializeField] Text myBalance;
    public Transform[] allButtons;
    [SerializeField] PhotonView photonView;
    [SerializeField] Swipe ticketToClaim;
    [SerializeField] Scrollbar ticketScrollBar;
    [SerializeField] GameObject buttonsPanel;
    [SerializeField] GameObject buttonPanelOffButton;
    [SerializeField] GameObject allNumberBoard;
    // [SerializeField] GameObject allNumberBoardONButton;
    [SerializeField] GameObject alreadyClaimed;
    [SerializeField] GameObject wrongClaimed;
    [SerializeField] GameObject rightClaimed;
    [SerializeField] GameObject quitPanel;

    public GameObject Cell;

    public Transform boardContainer;
    public List<Transform> boardContainerList = new List<Transform>();

    [SerializeField]
    public List<int> temp_numbers = new List<int>();

    [SerializeField]
    public Transform NumberCell;
    [SerializeField]
    public Transform EmptyCell;

    //[System.Serializable]

    private void start()
    {
        AdManager.instance.RequestInterstitial();
    }

    public class Tickets
    {
        public Transform ticket;
        public List<Transform> row1 = new List<Transform>(9);
        public List<Transform> row2 = new List<Transform>(9);
        public List<Transform> row3 = new List<Transform>(9);

        public List<Transform> row1Numbers = new List<Transform>();
        public List<Transform> row2Numbers = new List<Transform>();
        public List<Transform> row3Numbers = new List<Transform>();

        public List<Transform> sortedTicket = new List<Transform>(); 
    }

    [System.Serializable]
    public class TicketTest
    {
        public Transform ticket;
        public List<Transform> column1 = new List<Transform>(3);
        public List<Transform> column2 = new List<Transform>(3);
        public List<Transform> column3 = new List<Transform>(3);
        public List<Transform> column4 = new List<Transform>(3);
        public List<Transform> column5 = new List<Transform>(3);
        public List<Transform> column6 = new List<Transform>(3);
        public List<Transform> column7 = new List<Transform>(3);
        public List<Transform> column8 = new List<Transform>(3);
        public List<Transform> column9 = new List<Transform>(3);
    }

    public List<Tickets> allTickets;
    public List<TicketTest> allTestTickets;
    public List<GameObject> ticketsBoards;
    public List<string> sortedPlayers = new List<string>();

    public List<int> all15Numbers = new List<int>();
    int indexOfTicket;
    public int fullHouseCount;
    public string sceneToLoadAtTheEnd;
    public List<int> ticketIndex = new List<int>();

    //Added by Ankit
    //[HideInInspector]
    public bool isManual = true;
    [SerializeField] BoardManager boardManager;
    Action<bool> gameEnded;
    


    public enum GameType
    {
        Lazzy,
        Normal,
        Quick,
        Rapid,
        SixSlipGames,
        NormalSlipGames,
        ThemeGames,
        Tournament1,
        Tournament2,
        Tournament3
    }

    public GameType gameType;
    public string gameId;
    [SerializeField] GameObject resultPanel;
    List<string> claimedList = new List<string>();
    [SerializeField] GameObject resultPrefab;
    [SerializeField] GameObject resultContent;

    bool[,] ticketMatrix = new bool[3,9];
    int[,] ticketValueMatrix = new int[3, 9];
    Text[,] ticketTextMatrix = new Text[3, 9];
    public List<bool> claimStateList = new List<bool>();


    List<List<int>> mapper = new List<List<int>>();
    public bool isSoundOn;
    int currentTicket;
    [SerializeField] GameObject lateClaim;
    private bool resultShown = false;

    [Header("Claim Info") , Space(10)]
    [SerializeField] GameObject claimsContent;
    [SerializeField] GameObject claimDes;
    [SerializeField] GameObject claimInfoTicket;
    [SerializeField] InputField searchButtonIF;
    public CommunicateWithServer server;
    bool isResultShown = false;

    // Start is called before the first frame update
    void Awake()
    {
        //SendClaim("None");
        instance = this;
        photonView = GetComponent<PhotonView>();
        wrongClaimed.SetActive(false);
        rightClaimed.SetActive(false);
        alreadyClaimed.SetActive(false);
        for (int i = 0; i < Test.instance.numberOfTicket; i++)
        {
            ticketsBoards[i].SetActive(true);
        }

        gameEnded += GameEnded;
        gameEnded(boardManager.startGame);

        //InitializeClaims();
        if (SceneManager.GetActiveScene().name != "Theme Games")
            CreateBoardCell();

       // CreateTicketNumbers();

        CreateTicketTest(Test.instance.numberOfTicket);

        temp_numbers.Clear();
        buttonsPanel.SetActive(false);
        buttonPanelOffButton.SetActive(false);
        quitPanel.SetActive(false);
        fullHouseCount = 0;
        sceneToLoadAtTheEnd = "Null";
        //myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString();

        //if(SceneManager.GetActiveScene().name == "Tournament3")
        //    sortedPlayers = SaveSystem.LoadPlayer();

        if(Test.instance.currentGameName.Contains("Tournament") && !TournamentResult.instance.tournamentStarted)
        {
            SendClaim("None");
            TournamentResult.instance.tournamentStarted = true;
        }
        else if(!Test.instance.currentGameName.Contains("Tournament"))
            SendClaim("None");

        InitializeTournament();
        ClaimManager.instance.claimDescription = claimDes;
        ClaimManager.instance.GenerateClaims(claimsContent.transform);
        ClaimManager.instance.ticket = claimInfoTicket;
        searchButtonIF.onValueChanged.AddListener(delegate { ClaimManager.instance.Search(searchButtonIF); });

        //Result(NetworkScript.instance.roomName, true);
    }

    void InitializeTournament()
    {
        if (Test.instance.currentGameName.ToLower().Contains("tournament") && !TournamentResult.instance.tournamentInitialized)
        {
            foreach(Player p in PhotonNetwork.PlayerList)
            {
                
                string id = new string(p.NickName.Where(Char.IsDigit).ToArray());
                TournamentResult.instance.participant.Add(int.Parse(id), p.IsLocal);
                TournamentResult.instance.userIds.Add(id);
                
            }
            TournamentResult.instance.pincipleAmount = PhotonNetwork.PlayerList.Length * 100;
            TournamentResult.instance.tournamentInitialized = true;
        }
    }


 

    void WonPopUpOff()
    {
        wonPopUp.SetActive(false);
    }


    #region Added by Ankit For Automatic claim
    /// <summary>
    /// Automatically claim. Add by Ankit
    /// </summary>
    public void AutoCheck()
    {

       /* if (!isManual)
        {
            switch(gameType)
            {
                case GameType.Lazzy:
                    OnClickFastFive();
                    OnClickFourCorner();
                    OnClickTop();
                    OnClickBottom();
                    OnClickFullHousie();
                    OnClickMiddle();
                    break;
                case GameType.Normal:
                    OnClickFastFive();
                    OnClickFourCorner();
                    OnClickTop();
                    OnClickMiddle();
                    OnClickBottom();
                    OnClickFullHousie();
                    OnClickBreakfast();
                    OnClickLunch();
                    OnClickDinner();
                    break;
                case GameType.Quick:
                    OnClickFastFive();
                    OnClickFourCorner();
                    OnClickTop();
                    OnClickBottom();
                    OnClickFullHousie();
                    OnClickMiddle();
                    break;
                case GameType.Rapid:
                    OnClickFullHousie();
                    break;
                case GameType.Tournament1:
                    OnClickFullHousie();
                    break;
                case GameType.Tournament2:
                    OnClickFullHousie();
                    break;
                case GameType.Tournament3:
                    OnClickFullHousie();
                    break;
                case GameType.SixSlipGames:
                    OnClick18Lines();
                    OnClickBullseye();
                    OnClickDesert();
                    OnClickRailwayLines();
                    OnClickStarter();
                    OnClickTamprature();
                    OnClickTwinning();
                    break;
                case GameType.NormalSlipGames:
                    OnClickTwoPairColumn();
                    OnClickTwoPairRaw();
                    OnClick5Pandav();
                    OnClickOneTwoKaFour();
                    OnClick15Aug();
                    OnClick26Jan();
                    OnClickFourTwoKaOne();
                    OnClick1947();
                    OnClickAllEven();
                    OnClickAllOdd();
                    OnClickTwinning();
                    OnClickBamboo();
                    OnClickBrahma();
                    OnClickBreakfast();
                    OnClickLunch();
                    OnClickDinner();
                    OnClickCircle();
                    OnClickJawani();
                    OnClickTamprature();
                    OnClickDoubleTemp();
                    OnClickTripleTemp();
                    OnClickEclipse();
                    OnClickFirstHalf();
                    OnClickFourCornerCentre();
                    OnClickHumDoHumaareDo();
                    OnClickILoveYou();
                    OnClickKingsCorner();
                    OnClickQueenCorner();
                    OnClickLetterH();
                    OnClickLetterT();
                    OnClickMahesh();
                    OnClickBudhapa();
                    OnClickPlus();
                    OnClickPyramid();
                    OnClickReversePyramid();
                    OnClickSecondHalf();
                    OnClickShehnaiBidai();
                    OnClickVishnu();
                    OnClickZip();
                    OnClickZap();

                    break;
            }
            
        }*/
    }

   /* [PunRPC]
    void SendClaimState()
    {
        claimStateList.Clear();
        InitializeClaims();
        twoPairColumnchecked = claimStateList[0];
        twoPairRawChecked = claimStateList[1];
        fivePandavChecked = claimStateList[2];
        oneTwokaFourChecked = claimStateList[3];
        nineFourSevenChecked = claimStateList[4];
        fourTwoKaOneChecked = claimStateList[5];
        allEvenChecked = claimStateList[6];
        allOddChecked = claimStateList[7];
        bambooChecked = claimStateList[8];
        brahmaChecked = claimStateList[9];
        maheshChecked = claimStateList[10];
        vishnuChecked = claimStateList[11];
        budhapaChecked = claimStateList[12];
        circleChecked = claimStateList[13];
        jawaniChecked = claimStateList[14];
        fifteenAugChecked = claimStateList[15];
        twoSixJanChecked = claimStateList[16];
        doubleTempChecked = claimStateList[17];
        tripleTempChecked = claimStateList[18];
        eclipseChecked = claimStateList[19];
        firstHalfChecked = claimStateList[20];
        fourCornerCentreChecked = claimStateList[21];
        humDoOrHumareDoChecked = claimStateList[22];
        iLoveYouChecked = claimStateList[23];
        kingsConerChecked = claimStateList[24];
        queenCornerChecked = claimStateList[25];
        letterHChecked = claimStateList[26];
        letterTChecked = claimStateList[27];
        plusChecked = claimStateList[28];
        pyramidChecked = claimStateList[29];
        reversePyramidChecked = claimStateList[30];
        shehnaiBidaiChecked = claimStateList[31];
        secondHalfChecked = claimStateList[32];
        zipChecked = claimStateList[33];
        zapChecked = claimStateList[34];
        lineDoneChecked = claimStateList[35];
        bullEyeChecked = claimStateList[36];
        starterChecked = claimStateList[37];
        desertChecked = claimStateList[38];
        railwayLineChecked = claimStateList[39];
        temperatureChecked = claimStateList[40];
        twinningChecked = claimStateList[41];
        breakfastChecked = claimStateList[42];
        lunchChecked = claimStateList[43];
        dinnerChecked = claimStateList[44];
        fastFiveChecked = claimStateList[45];
        fourCornerChecked = claimStateList[46];
        topChecked = claimStateList[47];
        middleChecked = claimStateList[48];
        bottomChecked = claimStateList[49];
        fullHouseChecked = claimStateList[50];

        Debug.Log("Claim list first value to others " + claimStateList[45]);
    }*/


    [PunRPC]
    void ClaimBroadcast(string variableName , bool value)
    {
        this.GetType().GetField(variableName).SetValue(this, value);
        Debug.Log("************ after changed " + this.GetType().GetField(variableName).GetValue(this));
    }


    [SerializeField] GameObject[] onOff;

    public void AutoCheckToggle()
    {

        if (isManual)
        {
            isManual = false;
            onOff[0].SetActive(false);
            onOff[1].SetActive(true);
        }
        else
        {
            isManual = true;
            onOff[0].SetActive(true);
            onOff[1].SetActive(false);
        }
    }

    Transform GetBtton(string name)
    {
        foreach(Transform t in allButtons)
        {
            string s = t.name.ToLower();
            s = s.Replace(" ", string.Empty);

            if (s == name)
                return t;
        }
        return null;
    }
    #endregion

    [HideInInspector] public bool twoPairColumnchecked = false;
    public void OnClickTwoPairColumn(Transform twoPairColumn = null)
    {
        if(!twoPairColumnchecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            for (int i = 0; i < 9; i++)
            {
                if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled) //|| allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled)
                {
                    countEnable++;
                }
                else if(allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled)
                {
                    countEnable++;
                }
            }

            Debug.Log("column" + countEnable);
 
            if (countEnable >= 2)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(twoPairColumn != null)
                {
                    twoPairColumn.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    twoPairColumn.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(twoPairColumn.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("twopaircolumn");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(t.name, 0f));
                }
                
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Two Pair Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                twoPairColumnchecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "twoPairColumnchecked", true);
                ClaimBroadcast("twoPairColumnchecked", true);
                SendClaim("Two Pair Column");
                //
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                    
                }
                
            }
            
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }
    }

    [HideInInspector] public bool twoPairRawChecked = false;
    public void OnClickTwoPairRaw(Transform twoPairRaw = null)
    {
        if(!twoPairRawChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            /* for (int i = 0; i < 14; i++)
             {
                 if (allTickets[n].row1[i].name != "EmptyCell(Clone)" && allTickets[n].row1[i + 1].name != "EmptyCell(Clone)")
                 {
                     if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1[i + 1].GetComponent<Image>().isActiveAndEnabled)
                     {
                         countEnable++;
                         break;
                     }
                 }

                 if (allTickets[n].row2[i].name != "EmptyCell(Clone)" && allTickets[n].row2[i + 1].name != "EmptyCell(Clone)")
                 {
                     if (allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2[i + 1].GetComponent<Image>().isActiveAndEnabled)
                     {
                         countEnable++;
                         break;
                     }
                 }

                 if (allTickets[n].row3[i].name != "EmptyCell(Clone)" && allTickets[n].row3[i + 1].name != "EmptyCell(Clone)")
                 {
                     if (allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3[i + 1].GetComponent<Image>().isActiveAndEnabled)
                     {
                         countEnable++;
                         break;
                     }
                 }

             }*/
            for (int i = 0; i < 8; i++)
            {
                if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1[i+1].GetComponent<Image>().isActiveAndEnabled) //|| allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled)
                {
                    countEnable++;
                }
                else if (allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2[i + 1].GetComponent<Image>().isActiveAndEnabled) //|| allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled)
                {
                    countEnable++;
                }
                else if (allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3[i+1].GetComponent<Image>().isActiveAndEnabled)
                {
                    countEnable++;
                }
            }
            Debug.Log("claim" + countEnable);

            if (countEnable >= 2)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(twoPairRaw != null)
                {
                    twoPairRaw.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    twoPairRaw.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(twoPairRaw.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("twopairrow");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "Two Pair Raw Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                twoPairRawChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "twoPairRawChecked", true);
                ClaimBroadcast("twoPairRawChecked", true);
                SendClaim("Two Pair Row");
                //
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
            
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }


    [SerializeField] public bool fivePandavChecked = false;
    public void OnClick5Pandav(Transform pandav = null)
    {
        if(!fivePandavChecked)
        {
            if(isManual)
                clickAudio.Play();
            int n = currentTicket;
            int count = 0;
            int neededCount = 0;
            for (int i = 0; i < allTickets[n].row1Numbers.Count; i++)
            {
                if (allTickets[n].row1Numbers[i].GetChild(0).GetComponent<Text>().text.Contains('5'))
                    neededCount++;

                if (allTickets[n].row1Numbers[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[i].GetChild(0).GetComponent<Text>().text.Contains('5'))
                    count++;
            }

               

            for (int i = 0; i < allTickets[n].row2Numbers.Count; i++)
            {
                if (allTickets[n].row2Numbers[i].GetChild(0).GetComponent<Text>().text.Contains('5'))
                    neededCount++;

                if (allTickets[n].row2Numbers[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[i].GetChild(0).GetComponent<Text>().text.Contains('5'))
                    count++;
            }
               

            for (int i = 0; i < allTickets[n].row3Numbers.Count; i++)
            {
                if (allTickets[n].row3Numbers[i].GetChild(0).GetComponent<Text>().text.Contains('5'))
                    neededCount++;

                if (allTickets[n].row3Numbers[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[i].GetChild(0).GetComponent<Text>().text.Contains('5'))
                    count++;
            }

            Debug.Log("count " + count + " needed count " + neededCount);

            if (count == neededCount)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(pandav != null)
                {
                    pandav.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    pandav.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(pandav.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("5pandav");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(t.name, 0f));
                }

                if (SceneManager.GetActiveScene().name == "Normal")
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Won " + (PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f).ToString("0");
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                else
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.2f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Pandav Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                Invoke("RightClaimedOff", 2);
                fivePandavChecked = true;
                //;
                //if (!claimedList.Contains("Five Pandav"))
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "fivePandavChecked", true);
                ClaimBroadcast("fivePandavChecked", true);
                SendClaim("Five Pandav");
                //

            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            } 
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool oneTwokaFourChecked = false;
    public void OnClickOneTwoKaFour(Transform oneTwoKaFaour = null)
    {
        if(!oneTwokaFourChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (allTickets[n].row2Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[1].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[3].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(oneTwoKaFaour != null)
                {
                    oneTwoKaFaour.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    oneTwoKaFaour.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(oneTwoKaFaour.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("12ka4");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "One Two Ka Four Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                oneTwokaFourChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "oneTwokaFourChecked", true);
                ClaimBroadcast("oneTwokaFourChecked", true);
                //if (!claimedList.Contains("One Two Ka Four"))
                SendClaim("One Two Ka Four");
                

            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            }
            ;
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool nineFourSevenChecked = false;
    public void OnClick1947(Transform button = null)
    {
        if(!nineFourSevenChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            Debug.Log("current ticket: " + currentTicket);
            foreach (int i in new List<int> { 0, 3, 6, 8 })
            {
               
                if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled || allTickets[n].row1[i].name == "EmptyCell(Clone)")
                {
                    countEnable++;
                }

                if (allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled || allTickets[n].row2[i].name == "EmptyCell(Clone)")
                {
                    countEnable++;
                }

                if (allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled || allTickets[n].row3[i].name == "EmptyCell(Clone)")
                {
                    countEnable++;
                }
            }
            Debug.Log("value: " + countEnable);

            if (countEnable == 12)
            {
                if(isManual){rightClaimed.SetActive(true);}

                if(button != null)
                {
                    button.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    button.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(button.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("1947");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "1947 Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                nineFourSevenChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "nineFourSevenChecked", true);
                ClaimBroadcast("nineFourSevenChecked", true);
                SendClaim("1947");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
            
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool fourTwoKaOneChecked = false;
    public void OnClickFourTwoKaOne(Transform fourTwoKaOne = null)
    {
        if(!fourTwoKaOneChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (allTickets[n].row2Numbers[4].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[3].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (allTickets[n].row1Numbers[4].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[3].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[1].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(fourTwoKaOne != null)
                {
                    fourTwoKaOne.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    fourTwoKaOne.GetComponent<Button>().enabled = false;
                }
                else
                {
                    Transform t = GetBtton("42ka1");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                StartCoroutine(DisableButton(fourTwoKaOne.name, 0f));
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "42ka1 Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                fourTwoKaOneChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "fourTwoKaOneChecked", true);
                ClaimBroadcast("fourTwoKaOneChecked", true);
                SendClaim("Four Two Ka One");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            } 
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool allEvenChecked = false;
    public void OnClickAllEven(Transform allEven = null)
    {
        if(!allEvenChecked)
        {
            int countEven = 0;
            int countEnable = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (Int32.Parse(item.GetChild(0).GetComponent<Text>().text) % 2 == 0)
                {
                    countEven++;
                    if (item.GetComponent<Image>().isActiveAndEnabled)
                    {
                        countEnable++;
                    }
                }
            }

            if (countEnable == countEven)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(allEven != null)
                {
                    allEven.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    allEven.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(allEven.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("alleven");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "AllEven Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                allEvenChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "allEvenChecked", true);
                ClaimBroadcast("allEvenChecked", true);
                SendClaim("All Even");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
            
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }


    [HideInInspector] public bool allOddChecked = false;
    public void OnClickAllOdd(Transform allOdd = null)
    {

        if (!allOddChecked)
        {
            int countEven = 0;
            int countEnable = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (Int32.Parse(item.GetChild(0).GetComponent<Text>().text) % 2 != 0)
                {
                    countEven++;
                    if (item.GetComponent<Image>().isActiveAndEnabled)
                    {
                        countEnable++;
                    }
                }
            }

            if (countEnable == countEven)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(allOdd != null)
                {
                    allOdd.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    allOdd.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(allOdd.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("allodd");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "AllOdd Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                allOddChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "allOddChecked", true);
                ClaimBroadcast("allOddChecked", true);
                SendClaim("All Odd");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }

            
        }
    }

    [HideInInspector] public bool bambooChecked = false;
    public void OnClickBamboo(Transform bamboo = null)
    {
        if(!bambooChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(bamboo != null)
                {
                    bamboo.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    bamboo.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(bamboo.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("bamboo");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "Bamboo Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                bambooChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "bambooChecked", true);
                ClaimBroadcast("bambooChecked", true);
                SendClaim("Bamboo");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
            
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool brahmaChecked = false;
    public void OnClickBrahma(Transform brahma = null)
    {
        if(!brahmaChecked)
        {
            int countunder30 = 0;
            int countEnable = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (Int32.Parse(item.GetChild(0).GetComponent<Text>().text) <= 30)
                {
                    countunder30++;
                    if (item.GetComponent<Image>().isActiveAndEnabled)
                    {
                        countEnable++;
                    }
                }
            }

            if (countEnable == countunder30)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(brahma != null)
                {
                    brahma.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    brahma.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(brahma.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("brahma");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Brahma Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                brahmaChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "brahmaChecked", true);
                ClaimBroadcast("brahmaChecked", true);
                SendClaim("Brahma");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
            
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool maheshChecked = false;
    public void OnClickMahesh(Transform mahesh = null)
    {
        if(!maheshChecked)
        {
            int countbetween61to90 = 0;
            int countEnable = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (Int32.Parse(item.GetChild(0).GetComponent<Text>().text) >= 61 && Int32.Parse(item.GetChild(0).GetComponent<Text>().text) <= 90)
                {
                    countbetween61to90++;
                    if (item.GetComponent<Image>().isActiveAndEnabled)
                    {
                        countEnable++;
                    }
                }
            }

            if (countEnable == countbetween61to90)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(mahesh != null)
                {
                    mahesh.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    mahesh.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(mahesh.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("mahesh");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Mahesh Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                maheshChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "maheshChecked", true);
                ClaimBroadcast("maheshChecked", true);
                SendClaim("Mahesh");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
            
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool vishnuChecked = false;
    public void OnClickVishnu(Transform vishnu = null)
    {
        if(!vishnuChecked)
        {
            int countbetween31to60 = 0;
            int countEnable = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (Int32.Parse(item.GetChild(0).GetComponent<Text>().text) >= 31 && Int32.Parse(item.GetChild(0).GetComponent<Text>().text) <= 60)
                {
                    countbetween31to60++;
                    if (item.GetComponent<Image>().isActiveAndEnabled)
                    {
                        countEnable++;
                    }
                }
            }

            if (countEnable == countbetween31to60)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(vishnu != null)
                {
                    vishnu.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    vishnu.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(vishnu.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("vishnu");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Vishnu Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                vishnuChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "vishnuChecked", true);
                ClaimBroadcast("vishnuChecked", true);

                SendClaim("Vishnu");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }


    [HideInInspector] public bool budhapaChecked = false;
    public void OnClickBudhapa(Transform budhapa = null)
    {
        if(!budhapaChecked)
        {
            int countbetween46to90 = 0;
            int countEnable = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (Int32.Parse(item.GetChild(0).GetComponent<Text>().text) >= 46 && Int32.Parse(item.GetChild(0).GetComponent<Text>().text) <= 90)
                {
                    countbetween46to90++;
                    if (item.GetComponent<Image>().isActiveAndEnabled)
                    {
                        countEnable++;
                    }
                }
            }

            if (countEnable == countbetween46to90)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(budhapa != null)
                {
                    budhapa.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    budhapa.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(budhapa.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("budhapa");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Budhapa Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                budhapaChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "budhapaChecked", true);
                ClaimBroadcast("budhapaChecked", true);
                SendClaim("Budhapa");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool circleChecked = false;
    public void OnClickCircle(Transform circle = null)
    {
        if(!circle)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[3].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if (circle != null)
                {
                    circle.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    circle.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(circle.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("circle");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "Circle Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                circleChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "circleChecked", true);
                ClaimBroadcast("circleChecked", true);
                SendClaim("Circle");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool jawaniChecked = false;
    public void OnClickJawani(Transform jawani = null)
    {
        if(!jawaniChecked)
        {
            int countunder45 = 0;
            int countEnable = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (Int32.Parse(item.GetChild(0).GetComponent<Text>().text) <= 45)
                {
                    countunder45++;
                    if (item.GetComponent<Image>().isActiveAndEnabled)
                    {
                        countEnable++;
                    }
                }
            }

            if (countEnable == countunder45)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(jawani != null)
                {
                    jawani.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    jawani.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(jawani.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("jawani");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Jawani Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                jawaniChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "jawaniChecked", true);
                ClaimBroadcast("jawaniChecked", true);
                SendClaim("Jawani");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                } 
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool fifteenAugChecked = false;
    public void OnClick15Aug(Transform aug = null)
    {
        if(!fifteenAugChecked)
        {
            int countOne = 0;
            int countFive = 0;
            int n = currentTicket;
            Debug.Log("current selected ticket: " + currentTicket);
            foreach (Transform item in allTickets[n].sortedTicket)
            {
                Debug.Log(item);
                if (item.GetChild(0).GetComponent<Text>().text.Contains('1'))
                {
                    countOne++;
                }

                if (item.GetChild(0).GetComponent<Text>().text.Contains('5'))
                {
                    countFive++;
                }
            }

            if (countOne > 0 && countFive > 0)
            {
                //if(isManual){rightClaimed.SetActive(true);}
                if(aug != null)
                {
                    aug.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    aug.GetComponent<Button>().enabled = false;
                    //StartCoroutine(DisableButton(aug.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "15 Aug Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                fifteenAugChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "fifteenAugChecked", true);
                ClaimBroadcast("fifteenAugChecked", true);
                SendClaim("15 Aug");
                Debug.Log("*****************IF");
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                    Debug.Log("*****************else");
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool twoSixJanChecked = false;
    public void OnClick26Jan(Transform jan = null)
    {
        if(!twoSixJanChecked)
        {
            int countTwo = 0;
            int countSix = 0;
            int n = currentTicket;

            foreach (Transform item in allTickets[n].sortedTicket)
            {
                if (item.GetChild(0).GetComponent<Text>().text.ToString()[0] == '2' || item.GetChild(0).GetComponent<Text>().text.ToString()[1] == '2')
                {
                    countTwo++;
                }

                if (item.GetChild(0).GetComponent<Text>().text.ToString()[0] == '6' || item.GetChild(0).GetComponent<Text>().text.ToString()[1] == '6')
                {
                    countSix++;
                }
            }

            if (countTwo > 0 && countSix > 0)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(jan != null)
                {
                    jan.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    jan.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(jan.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("26jan");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "26Jan Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                twoSixJanChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "twoSixJanChecked", true);
                ClaimBroadcast("twoSixJanChecked", true);
                SendClaim("26 Jan");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }


    [HideInInspector] public bool doubleTempChecked = false;
    public void OnClickDoubleTemp(Transform doubleTemp = null)
    {
        if(!doubleTempChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].sortedTicket[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[13].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[14].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (countEnable == 1)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(doubleTemp != null)
                {
                    doubleTemp.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    doubleTemp.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(doubleTemp.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("doubletemp");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "Double Temp Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                doubleTempChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "doubleTempChecked", true);
                ClaimBroadcast("doubleTempChecked", true);
                SendClaim("Double Temp");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool tripleTempChecked = false;
    public void OnClickTripleTemp(Transform tripleTemp = null)
    {
        if(!tripleTempChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].sortedTicket[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[12].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[13].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[14].GetComponent<Image>().isActiveAndEnabled)
            {
                countEnable++;
            }

            if (countEnable == 1)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(tripleTemp != null)
                {
                    tripleTemp.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    tripleTemp.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(tripleTemp.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("tripletemp");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Triple Temp Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                tripleTempChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "tripleTempChecked", true);
                ClaimBroadcast("tripleTempChecked", true);
                SendClaim("Triple Temp");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool eclipseChecked = false;
    public void OnClickEclipse(Transform eclips = null)
    {
        if(!eclipseChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            foreach (int i in new List<int>() { 1, 2, 3 })
            {
                if (allTickets[n].row1Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

                if (allTickets[n].row3Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            if (allTickets[n].row2Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 8)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(eclips != null)
                {
                    eclips.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    eclips.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(eclips.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("eclipse");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Eclips Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                eclipseChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "eclipseChecked", true);
                ClaimBroadcast("eclipseChecked", true);
                SendClaim("Eclipse");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool firstHalfChecked = false;
    public void OnClickFirstHalf(Transform firstHalf = null)
    {
        if(!firstHalfChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            foreach (int i in new List<int>() { 0, 1, 2 })
            {
                if (allTickets[n].row1Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

                if (allTickets[n].row2Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

                if (allTickets[n].row3Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            if (countEnable == 9)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(firstHalf != null)
                {
                    firstHalf.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    firstHalf.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(firstHalf.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("firsthalf");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "First Half Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                firstHalfChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "firstHalfChecked", true);
                ClaimBroadcast("firstHalfChecked", true);
                SendClaim("First Claim");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool fourCornerCentreChecked = false;
    public void OnClickFourCornerCentre(Transform fourCornerCentre = null)
    {
        if(!fourCornerCentreChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            foreach (int i in new List<int>() { 0, 4 })
            {
                if (allTickets[n].row1Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

                if (allTickets[n].row3Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            if (allTickets[n].row2Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 5)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(fourCornerCentre != null)
                {
                    fourCornerCentre.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    fourCornerCentre.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(fourCornerCentre.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("fourcornercentre");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Four Corner Centre Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                fourCornerCentreChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "fourCornerCentreChecked", true);
                ClaimBroadcast("fourCornerCentreChecked", true);
                SendClaim("Four Corner Centre");
                
            }
            else
            {
                wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                Invoke("WrongClaimedOff", 2);
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool humDoOrHumareDoChecked = false;
    public void OnClickHumDoHumaareDo(Transform humDoHumareDo = null)
    {
        if(!humDoOrHumareDoChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;


            if (countEnable == 5)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(humDoHumareDo != null)
                {
                    humDoHumareDo.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    humDoHumareDo.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(humDoHumareDo.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("hum2humare2");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Hum Do Or Humare Do Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                humDoOrHumareDoChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "humDoOrHumareDoChecked", true);
                ClaimBroadcast("humDoOrHumareDoChecked", true);
                SendClaim("Hum Do Humare Do");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool iLoveYouChecked = false;
    public void OnClickILoveYou(Transform iLoveYou = null)
    {
        if(iLoveYouChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[3].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;


            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(iLoveYou != null)
                {
                    iLoveYou.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    iLoveYou.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(iLoveYou.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("iloveyou");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "ILoveYou Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                iLoveYouChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "iLoveYouChecked", true);
                ClaimBroadcast("iLoveYouChecked", true);
                SendClaim("I Love You");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        
    }

    [HideInInspector] public bool kingsConerChecked = false;
    public void OnClickKingsCorner(Transform kingsCorner = null)
    {
        if(!kingsConerChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;


            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(kingsCorner != null)
                {
                    kingsCorner.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    kingsCorner.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(kingsCorner.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("kingscorner");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Kings Corner Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                kingsConerChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "kingsConerChecked", true);
                ClaimBroadcast("kingsConerChecked", true);
                SendClaim("Kings Corners");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool queenCornerChecked = false;
    public void OnClickQueenCorner(Transform queenCorner = null)
    {
        if(!queenCornerChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[0].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;


            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(queenCorner != null)
                {
                    queenCorner.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    queenCorner.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(queenCorner.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("queenscorner");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text =  "Queen Corner Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                queenCornerChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "queenCornerChecked", true);
                ClaimBroadcast("queenCornerChecked", true);
                SendClaim("Queen Corner");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool letterHChecked = false;
    public void OnClickLetterH(Transform letterH = null)
    {
        if(!letterHChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            for (int i = 0; i < 5; i++)
            {
                if (allTickets[n].row2Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }
            if (allTickets[n].row1[0].name == "EmptyCell(Clone)")
                countEnable++;

            if (allTickets[n].row1[4].name == "EmptyCell(Clone)")
                countEnable++;

            if (allTickets[n].row3[0].name == "EmptyCell(Clone)")
                countEnable++;

            if (allTickets[n].row3[4].name == "EmptyCell(Clone)")
                countEnable++;

            if (allTickets[n].row1[0].name != "EmptyCell(Clone)")
                if (allTickets[n].row1[0].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

            if (allTickets[n].row1[4].name != "EmptyCell(Clone)")
                if (allTickets[n].row1[4].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

            if (allTickets[n].row3[0].name != "EmptyCell(Clone)")
                if (allTickets[n].row3[0].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

            if (allTickets[n].row3[4].name != "EmptyCell(Clone)")
                if (allTickets[n].row3[4].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;


            if (countEnable == 9)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(letterH != null)
                {
                    letterH.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    letterH.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(letterH.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("letterh");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "LetterH Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                letterHChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "letterHChecked", true);
                ClaimBroadcast("letterHChecked", true);
                SendClaim("Letter H");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool letterTChecked = false;
    public void OnClickLetterT(Transform letterT = null)
    {
        if(!letterTChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            for (int i = 0; i < 5; i++)
            {
                if (allTickets[n].row1Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }
            if (allTickets[n].row2[2].name == "EmptyCell(Clone)")
                countEnable++;

            if (allTickets[n].row3[4].name == "EmptyCell(Clone)")
                countEnable++;

            if (allTickets[n].row2[0].name != "EmptyCell(Clone)")
                if (allTickets[n].row2[0].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

            if (allTickets[n].row3[4].name != "EmptyCell(Clone)")
                if (allTickets[n].row3[4].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

            if (countEnable == 9)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(letterT != null)
                {
                    letterT.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    letterT.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(letterT.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("lettert");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "LetterT Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                letterTChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "letterTChecked", true);
                ClaimBroadcast("letterTChecked", true);
                SendClaim("Letter T");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool plusChecked = false;
    public void OnClickPlus(Transform plus = null)
    {
        if(!plusChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[3].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(plus != null)
                {
                    plus.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    plus.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(plus.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("plus");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Plus Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                plusChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "plusChecked", true);
                ClaimBroadcast("plusChecked", true);
                SendClaim("Plus");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool pyramidChecked = false;
    public void OnClickPyramid(Transform pyramid = null)
    {
        if(!pyramidChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[3].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(pyramid != null)
                {
                    pyramid.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    pyramid.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(pyramid.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("pyramid");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Pyramid Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                pyramidChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "pyramidChecked", true);
                ClaimBroadcast("pyramidChecked", true);
                SendClaim("Pyramid");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool reversePyramidChecked = false;
    public void OnClickReversePyramid(Transform reversePyramid = null)
    {
        if(!reversePyramidChecked)
        {
            int countEnable = 0;
            int n = currentTicket;

            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2Numbers[3].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(reversePyramid != null)
                {
                    reversePyramid.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    reversePyramid.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(reversePyramid.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("reversepyramid");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Reverse Pyramid Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                reversePyramidChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "reversePyramidChecked", true);
                ClaimBroadcast("reversePyramidChecked", true);
                SendClaim("Reverse Pyramid");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool shehnaiBidaiChecked = false;
    public void OnClickShehnaiBidai(Transform shehnaiBidai = null)
    {
        if(!shehnaiBidaiChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[1].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[2].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[3].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 2)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(shehnaiBidai != null)
                {
                    shehnaiBidai.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    shehnaiBidai.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(shehnaiBidai.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("shenaibidai");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Shehnai Bidai Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                shehnaiBidaiChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "shehnaiBidaiChecked", true);
                ClaimBroadcast("shehnaiBidaiChecked", true);
                SendClaim("Sehnai Bidai");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool secondHalfChecked = false;
    public void OnClickSecondHalf(Transform secondHalf = null)
    {
        if(!secondHalfChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            foreach (int i in new List<int>() { 2, 3, 4 })
            {
                if (allTickets[n].row1Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

                if (allTickets[n].row2Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;

                if (allTickets[n].row3Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            if (countEnable == 9)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(secondHalf != null)
                {
                    secondHalf.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    secondHalf.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(secondHalf.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("secondhalf");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Second Half Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                secondHalfChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "secondHalfChecked", true);
                ClaimBroadcast("secondHalfChecked", true);
                SendClaim("Second Half");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool zipChecked = false;
    public void OnClickZip(Transform zip = null)
    {
        if(!zipChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[1].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[3].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(zip != null)
                {
                    zip.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    zip.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(zip.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("zip");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Zip Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                zipChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "zipChecked", true);
                ClaimBroadcast("zipChecked", true);

                SendClaim("Zip");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool zapChecked = false;
    public void OnClickZap(Transform zap = null)
    {
        if(!zapChecked)
        {
            int countEnable = 0;
            int n = currentTicket;
            if (allTickets[n].row1Numbers[3].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row2Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[1].GetComponent<Image>().isActiveAndEnabled)
                countEnable++;

            if (countEnable == 3)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(zap != null)
                {
                    zap.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    zap.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(zap.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("zap");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Zap Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                zapChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "zapChecked", true);
                ClaimBroadcast("zapChecked", true);
                SendClaim("Zap");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    public void OnClick18Lines(Transform allLines = null)
    {
        allLines.gameObject.SetActive(!allLines.gameObject.activeSelf);
    }

    
    public void all18LineDone()
    {
        StartCoroutine(DisableButton(allButtons[0].name, 0f));
    }


    [HideInInspector] public bool lineDoneChecked = false;
    public void lineDone(bool done, string buttonName)
    {
        if(!lineDoneChecked)
        {
            if (done)
            {
                if(isManual){rightClaimed.SetActive(true);}
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Line Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                lineDoneChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "lineDoneChecked", true);
                ClaimBroadcast("lineDoneChecked", true);
                SendClaim("Line");
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool bullEyeChecked = false;
    public void OnClickBullseye(Transform bullsEye = null)
    {
        if(!bullEyeChecked)
        {
            int countEnable = 0;
            for (int n = 0; n < Test.instance.numberOfTicket; n++)
            {
                if (allTickets[n].row2Numbers[2].name != "EmptyCell(Clone)")
                    if (allTickets[n].row2Numbers[2].GetComponent<Image>().isActiveAndEnabled)
                        countEnable++;
            }

            if (countEnable == 6)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if (bullsEye != null)
                {
                    bullsEye.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    bullsEye.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(bullsEye.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("bullseye");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Bull's Eye Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                bullEyeChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "bullEyeChecked", true);
                ClaimBroadcast("bullEyeChecked", true);
                SendClaim("Bulls Eye");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool starterChecked = false;
    public void OnClickStarter(Transform starter = null)
    {
        if(!starterChecked)
        {
            int countEnable = 0;
            for (int n = 0; n < Test.instance.numberOfTicket; n++)
            {
                if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            if (countEnable == 6)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(starter != null)
                {
                    starter.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    starter.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(starter.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("starter");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Starter Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                starterChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "starterChecked", true);
                ClaimBroadcast("starterChecked", true);
                SendClaim("Starter");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool desertChecked = false;
    public void OnClickDesert(Transform desert = null)
    {

        if(!desertChecked)
        {
            int countEnable = 0;
            for (int n = 0; n < Test.instance.numberOfTicket; n++)
            {
                //if (allTickets[n].row2Numbers[4].name != "EmptyCell(Clone)")
                //    if (allTickets[n].row2Numbers[4].GetComponent<Image>().isActiveAndEnabled)
                //        countEnable++;
                if (allTickets[n].sortedTicket[0].GetComponent<Image>().isActiveAndEnabled)
                        countEnable++;
            }

            if (countEnable == 6)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(desert != null)
                {
                    desert.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    desert.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(desert.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("desert");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Desert Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                desertChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "desertChecked", true);
                ClaimBroadcast("desertChecked", true);
                SendClaim("Dessert");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool railwayLineChecked = false;
    public void OnClickRailwayLines(Transform railwayLines = null)
    {
        if(!railwayLineChecked)
        {
            int countEnable = 0;

            foreach (Transform item in allTickets[0].row1Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            foreach (Transform item in allTickets[5].row3Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            Debug.Log(countEnable);

            if (countEnable == 10)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(railwayLines != null)
                {
                    railwayLines.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    railwayLines.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(railwayLines.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("railwaylines");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Railway Line Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                railwayLineChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "railwayLineChecked", true);
                ClaimBroadcast("railwayLineChecked", true);
                SendClaim("Railway Line");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }



    }

    [HideInInspector] public bool temperatureChecked = false;
    public void OnClickTamprature(Transform tamprature = null)
    {

        if(!temperatureChecked)
        {
            int countEnable = 0;
            for (int n = 0; n < Test.instance.numberOfTicket; n++)
            {
                if (allTickets[n].sortedTicket[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].sortedTicket[14].GetComponent<Image>().isActiveAndEnabled)
                    countEnable++;
            }

            if (countEnable == 6)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(tamprature != null)
                {
                    tamprature.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    tamprature.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(tamprature.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("temprature");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Temperature Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                temperatureChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "temperatureChecked", true);
                ClaimBroadcast("temperatureChecked", true);
                SendClaim("Temperature");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool twinningChecked = false;
    public void OnClickTwinning(Transform twinning = null)
    {
        if(!twinningChecked)
        {
            int countEnable = 0;
            for (int n = 0; n < Test.instance.numberOfTicket; n++)
            {
                bool done = false;
                for (int i = 0; i < allTickets[n].row1.Count - 1; i++)
                    if (allTickets[n].row1[i].name != "EmptyCell(Clone)" && allTickets[n].row1[i + 1].name != "EmptyCell(Clone)")
                        if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1[i + 1].GetComponent<Image>().isActiveAndEnabled)
                            done = true;

                for (int i = 0; i < allTickets[n].row2.Count - 1; i++)
                    if (allTickets[n].row2[i].name != "EmptyCell(Clone)" && allTickets[n].row2[i + 1].name != "EmptyCell(Clone)")
                        if (allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row2[i + 1].GetComponent<Image>().isActiveAndEnabled)
                            done = true;

                for (int i = 0; i < allTickets[n].row3.Count - 1; i++)
                    if (allTickets[n].row3[i].name != "EmptyCell(Clone)" && allTickets[n].row3[i + 1].name != "EmptyCell(Clone)")
                        if (allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3[i + 1].GetComponent<Image>().isActiveAndEnabled)
                            done = true;

                if (done)
                    countEnable++;
            }

            if (countEnable == 6)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(twinning != null)
                {
                    twinning.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    twinning.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(twinning.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("twinning");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Twinning Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                twinningChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "twinningChecked", true);
                ClaimBroadcast("twinningChecked", true);
                SendClaim("Twinnig");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }

    }

    [HideInInspector] public bool breakfastChecked = false;
    public void OnClickBreakfast(Transform breakfast = null)
    {
        if(!breakfastChecked)
        {
            if(isManual)
                clickAudio.Play();

            int count = 0;
            int countEnabled = 0;
            int n = currentTicket;
            for (int i = 0; i < 3; i++)
                if (allTickets[n].row1[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 0; i < 3; i++)
                if (allTickets[n].row2[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 0; i < 3; i++)
                if (allTickets[n].row3[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 0; i < 3; i++)
                if (allTickets[n].row1[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            for (int i = 0; i < 3; i++)
                if (allTickets[n].row2[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            for (int i = 0; i < 3; i++)
                if (allTickets[n].row3[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            if (count == countEnabled)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(breakfast != null)
                {
                    breakfast.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    breakfast.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(breakfast.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("breakfast");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }



                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Breakfast Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                breakfastChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "breakfastChecked", true);
                ClaimBroadcast("breakfastChecked", true);
                SendClaim("Breakfast");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
               
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }

    }

    [HideInInspector] public bool lunchChecked = false;
    public void OnClickLunch(Transform lunch = null)
    {
        if (!lunchChecked)
        {
            if(isManual)
                clickAudio.Play();
            int count = 0;
            int countEnabled = 0;
            int n = currentTicket;
            for (int i = 3; i < 6; i++)
                if (allTickets[n].row1[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 3; i < 6; i++)
                if (allTickets[n].row2[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 3; i < 6; i++)
                if (allTickets[n].row3[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 3; i < 6; i++)
                if (allTickets[n].row1[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            for (int i = 3; i < 6; i++)
                if (allTickets[n].row2[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            for (int i = 3; i < 6; i++)
                if (allTickets[n].row3[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            if (count == countEnabled)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(lunch != null)
                {
                    lunch.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    lunch.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(lunch.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("lunch");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Lunch Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                lunchChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "lunchChecked", true);
                ClaimBroadcast("lunchChecked", true);
                SendClaim("Lunch");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }

    }

    [HideInInspector] public bool dinnerChecked = false;
    public void OnClickDinner(Transform dinner = null)
    {
        if(!dinnerChecked)
        {
            if(isManual)
                clickAudio.Play();
            int count = 0;
            int countEnabled = 0;
            int n = currentTicket;
            for (int i = 6; i < 9; i++)
                if (allTickets[n].row1[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 6; i < 9; i++)
                if (allTickets[n].row2[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 6; i < 9; i++)
                if (allTickets[n].row3[i].transform.name != "EmptyCell(Clone)")
                    count++;

            for (int i = 6; i < 9; i++)
                if (allTickets[n].row1[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row1[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            for (int i = 6; i < 9; i++)
                if (allTickets[n].row2[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row2[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            for (int i = 6; i < 9; i++)
                if (allTickets[n].row3[i].transform.name != "EmptyCell(Clone)")
                    if (allTickets[n].row3[i].GetComponent<Image>().isActiveAndEnabled)
                        countEnabled++;

            if (count == countEnabled)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(dinner != null)
                {
                    dinner.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    dinner.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(dinner.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("dinner");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Dinner Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                dinnerChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "dinnerChecked", true);
                ClaimBroadcast("dinnerChecked", true);
                SendClaim("Dinner");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

     public bool fastFiveChecked = false;
    public void OnClickFastFive(Transform fastFive = null)
    {
        Debug.Log("On click Fast Five");
        if (!fastFiveChecked)
        {

            if (isManual)
                clickAudio.Play();
            int n = currentTicket;
            Debug.Log("N is " + n);
            int count = 0;
            for (int i = 0; i < allTickets[n].row1Numbers.Count; i++)
            {
                Debug.Log("Counting...");
                if (allTickets[n].row1Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                {
                    Debug.Log("Got a count...");
                    count++;
                }
                    
            }
                

            for (int i = 0; i < allTickets[n].row2Numbers.Count; i++)
            {
                if (allTickets[n].row2Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                {
                    count++;
                }
                    
            }
                

            for (int i = 0; i < allTickets[n].row3Numbers.Count; i++)
            {
                if (allTickets[n].row3Numbers[i].GetComponent<Image>().isActiveAndEnabled)
                {
                    count++;

                }
            }
                

            Debug.Log("Value of count " + count);

            if (count >= 5)
            {
                rightClaimed.SetActive(true);
                if (fastFive != null)
                {
                    fastFive.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    fastFive.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(fastFive.name, 0f));
                }


                if (SceneManager.GetActiveScene().name == "Normal")
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Fast Five Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                else
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.2f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Fast Five Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                Invoke("RightClaimedOff", 2);
                fastFiveChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "fastFiveChecked", true);
                ClaimBroadcast("fastFiveChecked", true);
                SendClaim("Fast Five");
            }
            else
            {
                wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                Invoke("WrongClaimedOff", 2);
            }


        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }
        
    }

    [HideInInspector] public bool fourCornerChecked = false;
    public void OnClickFourCorner(Transform fourCorner = null)
    {
        if(!fourCornerChecked)
        {
            if(isManual)
                clickAudio.Play();
            int n = currentTicket;
            if (allTickets[n].row1Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row1Numbers[4].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[0].GetComponent<Image>().isActiveAndEnabled && allTickets[n].row3Numbers[4].GetComponent<Image>().isActiveAndEnabled)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(fourCorner != null)
                {
                    fourCorner.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    fourCorner.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(fourCorner.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("fourcorner");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }
                if (SceneManager.GetActiveScene().name == "Normal")
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Four Corner Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                else
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.2f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Four Corner Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                Invoke("RightClaimedOff", 2);
                fourCornerChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "fourCornerChecked", true);
                ClaimBroadcast("fourCornerChecked", true);
                SendClaim("Four Corner");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool topChecked = false;
    public void OnClickTop(Transform top = null)
    {
        if(!topChecked)
        {
            if(isManual)
                clickAudio.Play();
            int n = currentTicket;
            int count = 0;
            foreach (Transform item in allTickets[n].row1Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    count++;
            }

            if (count == 5)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(top != null)
                {
                    top.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    top.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(top.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("top");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                if (SceneManager.GetActiveScene().name == "Normal")
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Top Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                else
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.2f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Top Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                Invoke("RightClaimedOff", 2);
                topChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "topChecked", true);
                ClaimBroadcast("topChecked", true);
                SendClaim("Top");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool middleChecked = false;
    public void OnClickMiddle(Transform middle = null)
    {
        if(!middleChecked)
        {
            if(isManual)
                clickAudio.Play();
            int n = currentTicket;
            int count = 0;
            foreach (Transform item in allTickets[n].row2Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    count++;
            }

            if (count == 5)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(middle != null)
                {
                    middle.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    middle.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(middle.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("middle");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                if (SceneManager.GetActiveScene().name == "Normal")
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Middle Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                else
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.2f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Middle Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                Invoke("RightClaimedOff", 2);
                middleChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "middleChecked", true);
                ClaimBroadcast("middleChecked", true);
                SendClaim("Middle");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool bottomChecked = false;
    public void OnClickBottom(Transform bottom = null)
    {
        if(!bottomChecked)
        {
            if(isManual)
                clickAudio.Play();
            int n = currentTicket;
            int count = 0;
            foreach (Transform item in allTickets[n].row3Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    count++;
            }

            if (count == 5)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(bottom != null)
                {
                    bottom.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    bottom.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(bottom.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("bottom");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }

                if (SceneManager.GetActiveScene().name == "Normal")
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.6f * 0.125f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Bottom Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                else
                {
                    float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.2f;
                    PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                    myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                    wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Bottom Claimed";
                    wonPopUp.SetActive(true);
                    Invoke("WonPopUpOff", 3);
                }
                Invoke("RightClaimedOff", 2);
                bottomChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "bottomChecked", true);
                ClaimBroadcast("bottomChecked", true);
                SendClaim("Bottom");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [HideInInspector] public bool fullHouseChecked = false;
    public void OnClickFullHousie(Transform fullHouse = null)
    {
        if(!fullHouseChecked)
        {
            if(isManual)
                clickAudio.Play();
            int n = currentTicket;
            int count = 0;

            if (ticketIndex != null)
            {
                foreach (int item in ticketIndex)
                {
                    if (item == n)
                    {
                        alreadyClaimed.SetActive(true);
                        Invoke("AlreadyClaimedOff", 2);
                        return;
                    }
                }
            }

            foreach (Transform item in allTickets[n].row1Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    count++;
            }

            foreach (Transform item in allTickets[n].row2Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    count++;
            }

            foreach (Transform item in allTickets[n].row3Numbers)
            {
                if (item.GetComponent<Image>().isActiveAndEnabled)
                    count++;
            }

            if (count == 15)
            {
                if(isManual){rightClaimed.SetActive(true);}
                if(fullHouse != null)
                {
                    fullHouse.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    fullHouse.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(fullHouse.name, 0f));
                }
                else
                {
                    Transform t = GetBtton("fullhouse");
                    t.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                    t.GetComponent<Button>().enabled = false;
                    StartCoroutine(DisableButton(t.name, 0f));
                }
                fullHouseCount++;
                ticketIndex.Add(n);

                CoinDistributionForRapid(PlayerPrefs.GetString("UserID"));
                MyPos(PlayerPrefs.GetString("UserID"));
                photonView.RPC("MyPos", RpcTarget.Others, PlayerPrefs.GetString("UserID"));

                wonPopUp.transform.GetChild(0).GetComponent<Text>().text = "Full House " + fullHouseCount.ToString("0") + "Claimed";
                wonPopUp.SetActive(true);
                Invoke("WonPopUpOff", 3);
                Invoke("RightClaimedOff", 2);
                fullHouseChecked = true;
                photonView.RPC("ClaimBroadcast", RpcTarget.Others, "fullHouseChecked", true);
                ClaimBroadcast("fullHouseChecked", true);
                SendClaim("Full Housie");
                
            }
            else
            {
                if(isManual)
                {
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    Invoke("WrongClaimedOff", 2);
                }
                
            }
        }
        else
        {
            lateClaim.SetActive(true);
            Invoke("WrongClaimedOff", 2);
        }


    }

    [PunRPC]
    void MyPos(string playerId)
    {
        if(SceneManager.GetActiveScene().name == "Tournament2")
        {
            sortedPlayers.Add(playerId);
            SaveSystem.SavePlayer(sortedPlayers);
        }

        if(SceneManager.GetActiveScene().name == "Tournament3")
        {
            sortedPlayers.Remove(playerId);
        }
    }

    void CoinDistributionForRapid(string playerId)
    {
        if(SceneManager.GetActiveScene().name == "Tournament3")
        {
            int myPos = 0;
            foreach(Transform item in allButtons)
            {
                if(!item.GetComponent<Button>().enabled)
                    myPos++;
            }

            if(myPos == 1)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalInvestment") * 0.3f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 2)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalInvestment") * 0.2f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 3)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalInvestment") * 0.15f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            sortedPlayers.Remove(playerId);
        }

        if(SceneManager.GetActiveScene().name == "Normal")
        {
            int myPos = 0;
            foreach(Transform item in allButtons)
            {
                if(item.name == "FullHouse" && !item.GetComponent<Button>().enabled)
                    myPos++;
            }

            if(myPos == 1)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.4f * 0.6f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 2)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.4f * 0.4f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }
        }

        if(SceneManager.GetActiveScene().name == "Rapid")
        {
            int myPos = 0;
            foreach(Transform item in allButtons)
            {
                if(!item.GetComponent<Button>().enabled)
                    myPos++;
            }

            if(myPos == 1)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.3f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 2)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.25f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 3)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.2f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 4)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.15f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 5)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalPrize") * 0.10f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

        }

        if(SceneManager.GetActiveScene().name == "Lazzy")
        {
            int myPos = 0;
            foreach(Transform item in allButtons)
            {
                if(item.name == "FullHouse" && !item.GetComponent<Button>().enabled)
                    myPos++;
            }

            if(myPos == 1)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.6f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }

            if(myPos == 2)
            {
                float coin = PlayerPrefs.GetInt("UserCoin") + (PlayerPrefs.GetFloat("TotalPrize") / 2) * 0.4f;
                PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
            }
        }
    }

    IEnumerator DisableButton(string buttonName, float delay)
    {
        yield return new WaitForSeconds(delay);
        //photonView.RPC("DisableButtonForOthers", RpcTarget.Others, buttonName);
        DisableButtonForOthers(buttonName);
    }

    [PunRPC]
    void DisableButtonForOthers(string buttonName)
    {
        int count = 0;

        foreach(Transform item in allButtons)
        {
            if(item.name == buttonName)
            {
                item.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
                item.GetComponent<Button>().enabled = false;
                item.GetChild(0).gameObject.SetActive(true);
            }

            if(!item.GetComponent<Button>().enabled)
                count++;
        }

        if(count == allButtons.Length)
        {
            switch(SceneManager.GetActiveScene().name)
            {
                case "Tournament1":
                    if(fullHouseCount > 0)
                    {
                        wrongClaimed.GetComponent<Text>().text = "Prepare For Next Level";
                        wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                        Invoke("LoadNextLevel", 3);
                    }

                    else
                    {
                        //wrongClaimed.GetComponent<Text>().text = "GAME OVER";
                        wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                        //Result(NetworkScript.instance.roomName, true);
                    }
                    break;
                
                case "Tournament2":
                    if(fullHouseCount > 0)
                    {
                        wrongClaimed.GetComponent<Text>().text = "Prepare For Next Level";
                        wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                        Invoke("LoadNextLevel", 3);
                    }

                    else
                    {
                        //wrongClaimed.GetComponent<Text>().text = "GAME OVER";
                        wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                        //Result(NetworkScript.instance.roomName, true);
                    }
                    break;
                
                case "Tournament3":
                    if(fullHouseCount > 0)
                    {
                        //wrongClaimed.GetComponent<Text>().text = "GAME OVER";
                        wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                        //Result(NetworkScript.instance.roomName, true);
                    }
                    else
                    {
                        if(sortedPlayers[0] == PlayerPrefs.GetString("UserID"))
                        {
                            float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalInvestment") * 0.125f;
                            PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                            myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                        }

                        if(sortedPlayers[2] == PlayerPrefs.GetString("UserID"))
                        {
                            float coin = PlayerPrefs.GetInt("UserCoin") + PlayerPrefs.GetFloat("TotalInvestment") * 0.1f;
                            PlayerPrefs.SetInt("UserCoin", Int32.Parse(coin.ToString("0")));Reward((int)coin, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
                            myBalance.text = PlayerPrefs.GetInt("UserCoin").ToString("0");
                        }
                    }
                    break;

                default:
                    //wrongClaimed.GetComponent<Text>().text = "GAME OVER";
                    wrongClaimed.SetActive(true); ticketsBoards[currentTicket].transform.GetChild(3).gameObject.SetActive(true);
                    //Result(NetworkScript.instance.roomName, true);
                    break;
            }
        }
    }


    void GameOver()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Login");

        AdManager.instance.ShowInterstitial();

    }

    void LoadNextLevel()
    {
        switch(SceneManager.GetActiveScene().name)
        {
            case "Tournament1":
                PhotonNetwork.LoadLevel("Tournament2");
                break;
            
            case "Tournament2":
                PhotonNetwork.LoadLevel("Tournament3");
                break;
        }
    }

    void AlreadyClaimedOff()
    {
        alreadyClaimed.SetActive(false);
    }

    void WrongClaimedOff()
    {
        wrongClaimed.SetActive(false);
        lateClaim.SetActive(false);
    }

    void RightClaimedOff()
    {
        rightClaimed.SetActive(false);
    }

    public void CreateBoardCell()
    {

        for (int i = 1; i < 91; i++)
        {
            //Debug.Log("Cell Created" + i);
            GameObject cellObject = Instantiate(Cell.gameObject);
            cellObject.transform.SetParent(boardContainer);
            cellObject.GetComponent<RectTransform>().localScale = Vector3.one;
            cellObject.transform.GetChild(1).GetComponent<Text>().text = i.ToString();
            boardContainerList.Add(cellObject.transform);
            temp_numbers.Add(i);
        }

    }


    public void CreateBoardCellRPC()
    {
        // Player.GetComponent<TambolaPlayer>().PlayerNumbers.Clear();
        for (int i = 1; i < 91; i++)
        {
            GameObject cellObject = Instantiate(Cell.gameObject);
            cellObject.transform.SetParent(boardContainer);
            cellObject.transform.GetChild(1).GetComponent<Text>().text = i.ToString();
            temp_numbers.Add(i);
        }

    }


    public List<List<int>> mainSplitHolder = new List<List<int>>();
    List<int> fourTicketPossibility = new List<int>() {3,2,2,2};
    List<int> fiveTicketPossibility = new List<int>() {3,2,2,1,1};
    List<int> sixTicketPossibility1 = new List<int>() {3,2,1,1,1,1};
    List<int> sixTicketPossibility2 = new List<int>() {2,2,2,1,1,1};
    public void CreateTicketNumbers()
    {
        // Row 1 
        // Add Numbers to Row 1 Dictionery
        // Player.GetComponent<TambolaPlayer>().PlayerNumbers.Clear();

        //for (int n = 0; n < Test.instance.numberOfTicket * 15; n++)


        for (int n = 0; n < 15; n++)
        {
            int nRandomPosition = UnityEngine.Random.Range(0, temp_numbers.Count);

            all15Numbers.Add(temp_numbers[nRandomPosition]);
            temp_numbers.Remove(temp_numbers[nRandomPosition]);
        }

        if (Test.instance.numberOfTicket == 0)
            Test.instance.numberOfTicket = 1;

        CreateTypeDictionary();
        for (int n = 0; n < Test.instance.numberOfTicket; n++)
        {
            CreateMapper(n);
            InitializeTicketMatrix();
            Row1(n);
            Row2(n);
            Row3(n);
            RowNumberCollection(n);
            //Sorting(n);
            //mainSplitHolder.Clear();
            //FinalTicket();
            //InitializeTicketMatrix();
            //mapper.Clear();
        }
        
        mainSplitHolder.Clear();
    }

    void InitializeTicketMatrix()
    {
        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                ticketMatrix[i,j] = false;
                ticketValueMatrix[i , j] = -1;
                ticketTextMatrix[i, j] = null;
            }
        }
    }

    void CreateMapper(int n)
    {
        List<int> firstRow = new List<int>();
        List<int> secRow = new List<int>();
        List<int> thirdRow = new List<int>();
        List<int> mapTrackerNegation = new List<int>() {0 ,1,2,3,4,5,6,7,8 };
        for (int i = 0; i < 3; i++)
        {
            List<int> tempRow = new List<int>();
            for (int j = 0; j < 9; j++)
            {
                tempRow.Add(j);
            }
            mapper.Add(tempRow);
            
        }


        if (Test.instance.numberOfTicket >3)
        {
            /*switch(n)
            {
                case 4:
                    for (int i = 0; i < 4; i++)
                    {
                        int r1 = mapper[0][UnityEngine.Random.Range(0, mapper[0].Count)];
                        mapper[0].Remove(r1);
                        firstRow.Add(r1);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        int r1 = mapper[1][UnityEngine.Random.Range(0, mapper[1].Count)];
                        mapper[1].Remove(r1);
                        secRow.Add(r1);
                    }

                    mapper[2].Clear();
                    thirdRow = firstRow.Intersect(secRow).ToList();
                    for (int i = 0; i < thirdRow.Count; i++)
                    {
                        Debug.Log("-->>>" + thirdRow[i]);
                        mapper[2].Add(thirdRow[i]);
                        mapTrackerNegation.Remove(thirdRow[i]);
                    }

                    if (mapper[2].Count < 5)
                    {
                        for (int i = mapper[2].Count; i <= 5; i++)
                        {
                            int r1 = mapTrackerNegation[UnityEngine.Random.Range(0, mapTrackerNegation.Count)];
                            mapper[2].Add(r1);
                            mapTrackerNegation.Remove(r1);
                        }
                    }

                    break;
                case 5:

                    for (int i = 0; i < 4; i++)
                    {
                        int r1 = mapper[0][UnityEngine.Random.Range(0, mapper[0].Count)];
                        mapper[0].Remove(r1);
                        firstRow.Add(r1);
                    }



                    for (int i = 0; i < 4; i++)
                    {
                        int r1 = mapper[1][UnityEngine.Random.Range(0, mapper[1].Count)];
                        mapper[1].Remove(r1);
                        secRow.Add(r1);
                    }

                    break;
                case 6:
                    break;
            }*/

            /* for(int i = 0; i < 4; i++)
             {
                 int r1 = mapper[0][UnityEngine.Random.Range(0, mapper[0].Count)];
                 mapper[0].Remove(r1);
                 firstRow.Add(r1);
             }


             for (int i = 0; i < 4; i++)
             {
                 int r1 = mapper[1][UnityEngine.Random.Range(0, mapper[1].Count)];
                 mapper[1].Remove(r1);
                 secRow.Add(r1);
             }

             mapper[2].Clear();
             thirdRow = firstRow.Intersect(secRow).ToList();
             for(int i = 0; i < thirdRow.Count; i++)
             {
                 Debug.Log("-->>>"+thirdRow[i]);
                 mapper[2].Add(thirdRow[i]);
                 mapTrackerNegation.Remove(thirdRow[i]);
             }

             foreach(int i in mapper[0].Intersect(mapper[1].ToList()))
             {
                 mapTrackerNegation.Remove(i);
             }

             if (mapper[2].Count < 5)
             {
                 for(int i = mapper[2].Count; i <= 5; i++)
                 {
                     int r1 = mapTrackerNegation[UnityEngine.Random.Range(0, mapTrackerNegation.Count)];
                     mapper[2].Add(r1);
                     mapTrackerNegation.Remove(r1);
                 }
             }*/

            for (int i = 0; i < 4; i++)
            {
                int r1 = mapper[0][UnityEngine.Random.Range(0, mapper[0].Count)];
                mapper[0].Remove(r1);
                firstRow.Add(r1);
            }

            mapper[1].Clear();
            for(int i = 0; i < firstRow.Count; i++)
            {
                mapper[1].Add(firstRow[i]);
                mapTrackerNegation.Remove(firstRow[i]);
            }

            mapper[1].Add(mapTrackerNegation[UnityEngine.Random.Range(0, mapTrackerNegation.Count)]);

            mapper[2].Clear();
            mapTrackerNegation = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            thirdRow = mapper[0].Intersect(mapper[1]).ToList();
            foreach (int i in thirdRow)
                mapTrackerNegation.Remove(i);
            for(int i = 0; i < mapTrackerNegation.Count;i++)
            {
                mapper[2].Add(mapTrackerNegation[i]);
            }

            if(mapper[2].Count < 5)
            {
                int r1 = thirdRow[UnityEngine.Random.Range(0, thirdRow.Count)];
                mapper[2].Add(r1);
            }
        }
    }

    List<int> FindEmptyRows()
    {
        List<int> temp = new List<int>();
        if (ticketMatrix[0, 0] == false && ticketMatrix[1, 0] == false)
            temp.Add(0);
        if (ticketMatrix[0, 1] == false && ticketMatrix[1, 1] == false)
            temp.Add(1);
        if (ticketMatrix[0, 2] == false && ticketMatrix[1, 2] == false)
            temp.Add(2);
        if (ticketMatrix[0, 3] == false && ticketMatrix[1, 3] == false)
            temp.Add(3);
        if (ticketMatrix[0, 4] == false && ticketMatrix[1, 4] == false)
            temp.Add(4);
        if (ticketMatrix[0, 5] == false && ticketMatrix[1, 5] == false)
            temp.Add(5);
        if (ticketMatrix[0, 6] == false && ticketMatrix[1, 6] == false)
            temp.Add(6);
        if (ticketMatrix[0, 7] == false && ticketMatrix[1, 7] == false)
            temp.Add(7);
        if (ticketMatrix[0, 8] == false && ticketMatrix[1, 8] == false)
            temp.Add(8);

        return temp;
    }

    void CreateTypeDictionary()
    {
        mainSplitHolder.Add(GenerateTempSplitDict(1, 10));
        mainSplitHolder.Add(GenerateTempSplitDict(10, 20));
        mainSplitHolder.Add(GenerateTempSplitDict(20, 30));
        mainSplitHolder.Add(GenerateTempSplitDict(30, 40));
        mainSplitHolder.Add(GenerateTempSplitDict(40, 50));
        mainSplitHolder.Add(GenerateTempSplitDict(50, 60));
        mainSplitHolder.Add(GenerateTempSplitDict(60, 70));
        mainSplitHolder.Add(GenerateTempSplitDict(70, 80));
        mainSplitHolder.Add(GenerateTempSplitDict(80, 90));
    }

    //List<int> totalCell = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

    void Row1(int n)
    {

        List<int> totalCell = new List<int>() {0, 1, 2, 3, 4, 5, 6, 7, 8};
        //List<int> totalCell = mapper[0];
        for(int i = 0; i < 5; i++)
        {
            //int numberCell = totalCell[UnityEngine.Random.Range(0, totalCell.Count)];
            int numberCell = mapper[0][UnityEngine.Random.Range(0, mapper[0].Count)];
            totalCell.Remove(numberCell);
            mapper[0].Remove(numberCell);
            ticketMatrix[0, numberCell] = true;


            int numberGot = GetNumberAsPerColumn(numberCell); //all15Numbers[UnityEngine.Random.Range(0, all15Numbers.Count)];
            //int numberGot = all15Numbers[UnityEngine.Random.Range(0, all15Numbers.Count)];
            //Debug.Log("Number Got " + numberGot);
            //all15Numbers.Remove(numberGot);

            ticketValueMatrix[0, numberCell] = numberGot;

            Transform t = Instantiate(NumberCell);
            ticketTextMatrix[0, numberCell] = t.GetChild(0).GetComponent<Text>();
            t.GetChild(0).GetComponent<Text>().text = numberGot.ToString();
            t.name = numberGot.ToString();
            // t.transform.SetParent(content, false);
            allTickets[n].row1[numberCell] = t;

        }
        for(int i = 0; i < 4; i++)
        {
            int emptyCell = totalCell[UnityEngine.Random.Range(0, totalCell.Count)];
            totalCell.Remove(emptyCell);
            Transform t = Instantiate(EmptyCell);
            // t.transform.SetParent(content, false);
            allTickets[n].row1[emptyCell] = t;
        }

        foreach(Transform t in allTickets[n].row1)
        {
            t.transform.SetParent(allTickets[n].ticket, false);
        }
    }

    void Row2(int n)
    {
        List<int> totalCell = new List<int>() {0, 1, 2, 3, 4, 5, 6, 7, 8};
        //List<int> totalCell = mapper[1];
        for(int i = 0; i < 5; i++)
        {
            //int numberCell = totalCell[UnityEngine.Random.Range(0, totalCell.Count)];
            int numberCell = mapper[1][UnityEngine.Random.Range(0, mapper[1].Count)];
            totalCell.Remove(numberCell);
            mapper[1].Remove(numberCell);
            ticketMatrix[1, numberCell] = true;
            int numberGot = GetNumberAsPerColumn(numberCell); //all15Numbers[UnityEngine.Random.Range(0, all15Numbers.Count)];
            //Debug.Log("Number Got " + numberGot);
            //all15Numbers.Remove(numberGot);

            ticketValueMatrix[1, numberCell] = numberGot;
            Transform t = Instantiate(NumberCell);
            ticketTextMatrix[1, numberCell] = t.GetChild(0).GetComponent<Text>();
            t.GetChild(0).GetComponent<Text>().text = numberGot.ToString();
            t.name = numberGot.ToString();
            // t.transform.SetParent(content, false);
            allTickets[n].row2[numberCell] = t;

            
        }
        for(int i = 0; i < 4; i++)
        {
            int emptyCell = totalCell[UnityEngine.Random.Range(0, totalCell.Count)];
            totalCell.Remove(emptyCell);
            Transform t = Instantiate(EmptyCell);
            // t.transform.SetParent(content, false);
            allTickets[n].row2[emptyCell] = t;
        }

        foreach(Transform t in allTickets[n].row2)
        {
            t.transform.SetParent(allTickets[n].ticket, false);
        }
    }

    void Row3(int n)
    {
        List<int> totalCell = new List<int>() {0, 1, 2, 3, 4, 5, 6, 7, 8};
        //List<int> totalCell = mapper[2];
        List<int> empty = new List<int>();
        if (Test.instance.numberOfTicket < 3)
        {
            empty = FindEmptyRows();
        }

        for(int i = 0; i < 5; i++)
        {
            int numberCell;
            
            if(empty.Count > 0)
            {
                numberCell = empty[UnityEngine.Random.Range(0, empty.Count)];
                empty.Remove(numberCell);
            }
            else
            {
                //numberCell = totalCell[UnityEngine.Random.Range(0, totalCell.Count)];
                numberCell = mapper[2][UnityEngine.Random.Range(0, mapper[2].Count)];
                mapper[2].Remove(numberCell);
            }

            totalCell.Remove(numberCell);

            Debug.Log("Cell got " + numberCell);

            int numberGot = GetNumberAsPerColumn(numberCell);//all15Numbers[UnityEngine.Random.Range(0, all15Numbers.Count)];
            ticketValueMatrix[2, numberCell] = numberGot;
            Transform t = Instantiate(NumberCell);
            ticketTextMatrix[2, numberCell] = t.GetChild(0).GetComponent<Text>();
            t.GetChild(0).GetComponent<Text>().text = numberGot.ToString();
            t.name = numberGot.ToString();
            // t.transform.SetParent(content, false);
            allTickets[n].row3[numberCell] = t;
        }


        for (int i = 0; i < 4; i++)
        {
            int emptyCell = totalCell[UnityEngine.Random.Range(0, totalCell.Count)];
            totalCell.Remove(emptyCell);
            Transform t = Instantiate(EmptyCell);
            // t.transform.SetParent(content, false);
            allTickets[n].row3[emptyCell] = t;
        }

        foreach(Transform t in allTickets[n].row3)
        {
            t.transform.SetParent(allTickets[n].ticket, false);
        }
    }


    int GetNumberAsPerColumn(int cellNUmber)
    {
        int numberGot = 0;
        switch (cellNUmber)
        {
            case 0:
                numberGot = mainSplitHolder[0][UnityEngine.Random.Range(0, mainSplitHolder[0].Count)];
                mainSplitHolder[0].Remove(numberGot);
                break;
            case 1:
                numberGot = mainSplitHolder[1][UnityEngine.Random.Range(0, mainSplitHolder[1].Count)];
                mainSplitHolder[1].Remove(numberGot);
                break;
            case 2:
                numberGot = mainSplitHolder[2][UnityEngine.Random.Range(0, mainSplitHolder[2].Count)];
                mainSplitHolder[2].Remove(numberGot);
                break;
            case 3:
                numberGot = mainSplitHolder[3][UnityEngine.Random.Range(0, mainSplitHolder[3].Count)];
                mainSplitHolder[3].Remove(numberGot);
                break;
            case 4:
                numberGot = mainSplitHolder[4][UnityEngine.Random.Range(0, mainSplitHolder[4].Count)];
                mainSplitHolder[4].Remove(numberGot);
                break;
            case 5:
                numberGot = mainSplitHolder[5][UnityEngine.Random.Range(0, mainSplitHolder[5].Count)];
                mainSplitHolder[5].Remove(numberGot);
                break;
            case 6:
                numberGot = mainSplitHolder[6][UnityEngine.Random.Range(0, mainSplitHolder[6].Count)];
                mainSplitHolder[6].Remove(numberGot);
                break;
            case 7:
                numberGot = mainSplitHolder[7][UnityEngine.Random.Range(0, mainSplitHolder[7].Count)];
                mainSplitHolder[7].Remove(numberGot);
                break;
            case 8:
                numberGot = mainSplitHolder[8][UnityEngine.Random.Range(0, mainSplitHolder[8].Count)];
                mainSplitHolder[8].Remove(numberGot);
                break;
        }
        return numberGot;
    }
    
    void FinalTicket()
    {
        List<int> temp = new List<int>();

        //for(int i = 0; i < 9; i++)
        //{
        //    for(int j =0; j < 3;j++)
        //    {
        //        Debug.Log("Value " + ticketValueMatrix[j, i] + " Text " + ticketTextMatrix[j, i]);
        //    }
        //}    


        for (int i = 0; i < 9; i++)
        {
            
            for(int j = 0; j < 3; j++)
            {
                int v = ticketValueMatrix[j, i];
                if(v != -1)
                    temp.Add(v);
            }

            temp.Sort();

            //List<Text> tempText = new List<Text>();
            for (int j = 0; j < 3; j++)
            {
                if(ticketTextMatrix[j , i] != null)
                {
                    ticketTextMatrix[j, i].text = temp[0].ToString();
                    temp.RemoveAt(0);
                    //tempText.Add(ticketTextMatrix[j, i]);
                }
            }
            //Debug.Log("Value count " + temp.Count + " text count " + tempText.Count);
            temp.Clear();
        }

        
    }


    List<int> GenerateTempSplitDict(int minValue , int maxValue)
    {
        List<int> value = new List<int>();
        for(int i = minValue; i < maxValue; i++)
        {
            value.Add(i);
        }
        return value;
    }


    void RowNumberCollection(int n)
    {
        foreach(Transform item in allTickets[n].row1)
        {
            if(item.name != "EmptyCell(Clone)")
            {
                allTickets[n].row1Numbers.Add(item);
            }
        }

        foreach(Transform item in allTickets[n].row2)
        {
            if(item.name != "EmptyCell(Clone)")
            {
                allTickets[n].row2Numbers.Add(item);
            }
        }

        foreach(Transform item in allTickets[n].row3)
        {
            if(item.name != "EmptyCell(Clone)")
            {
                allTickets[n].row3Numbers.Add(item);
            }
        }
    }

    public void OnClickClaimButton(int index)
    {
        currentTicket = index;
        clickAudio.Play();
        buttonsPanel.SetActive(true);
        buttonPanelOffButton.SetActive(true);
        ticketToClaim.scroll_pos = index * 0.2f;
    }

    public void OnClickDownButtonPanel()
    {
        clickAudio.Play();
        buttonsPanel.SetActive(false);
        buttonPanelOffButton.SetActive(false);
    }

    public void OnTicketValueChange()
    {
        buttonsPanel.SetActive(false);
        buttonPanelOffButton.SetActive(false);
    }

    public void OnClickQuitButton()
    {
        clickAudio.Play();
        quitPanel.SetActive(true);
    }

    public void OnClickYesQuitButton()
    {
        clickAudio.Play();

        PhotonNetwork.Disconnect();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Login");
    }

    public void OnClickNoQuitButton()
    {
        clickAudio.Play();
        quitPanel.SetActive(false);
    }

    public void DownButtonPanel()
    {
        if(ticketScrollBar.value > 0.05f && ticketScrollBar.value < 0.15f)
        {
            buttonsPanel.SetActive(false);
            buttonPanelOffButton.SetActive(false);
        }

        if(ticketScrollBar.value > 0.25f && ticketScrollBar.value < 0.35f)
        {
            buttonsPanel.SetActive(false);
            buttonPanelOffButton.SetActive(false);
        }

        if(ticketScrollBar.value > 0.45f && ticketScrollBar.value < 0.55f)
        {
            buttonsPanel.SetActive(false);
            buttonPanelOffButton.SetActive(false);
        }

        if(ticketScrollBar.value > 0.65f && ticketScrollBar.value < 0.75f)
        {
            buttonsPanel.SetActive(false);
            buttonPanelOffButton.SetActive(false);
        }

        if(ticketScrollBar.value > 0.85f && ticketScrollBar.value < 0.95f)
        {
            buttonsPanel.SetActive(false);
            buttonPanelOffButton.SetActive(false);
        }
    }

    public void OnClickNumberBoardOFF()
    {
        allNumberBoard.SetActive(false);
    }

    public void OnClickNumberBoardON()
    {
        allNumberBoard.SetActive(true);
    }

    void Sorting(int n)
    {
        foreach(Transform item in allTickets[n].row1Numbers)
            allTickets[n].sortedTicket.Add(item);

        foreach(Transform item in allTickets[n].row2Numbers)
            allTickets[n].sortedTicket.Add(item);
        
        foreach(Transform item in allTickets[n].row3Numbers)
            allTickets[n].sortedTicket.Add(item);

        for(int j = 0; j <= allTickets[n].sortedTicket.Count - 2; j++)
        {
            for(int i = 0; i <= allTickets[n].sortedTicket.Count - 2; i++)
            {
                if(Int32.Parse(allTickets[n].sortedTicket[i].GetChild(0).GetComponent<Text>().text.ToString()) < Int32.Parse(allTickets[n].sortedTicket[i + 1].GetChild(0).GetComponent<Text>().text.ToString()))
                {
                    Transform c = allTickets[n].sortedTicket[i];
                    allTickets[n].sortedTicket[i] = allTickets[n].sortedTicket[i + 1];
                    allTickets[n].sortedTicket[i + 1] = c;
                }
            }
        }
    }


    void SendClaim(string claimName)
    {
        //StartCoroutine(SendClaimData(claimName));

        var url = StaticDetatils.baseUrl + "Api/Claim/create.php";
        ClaimData claimData = new ClaimData()
        {
            game_id = NetworkScript.instance.roomName,
            user_id = PlayerPrefs.GetString("UserID"),
            claim_name = claimName
        };

        Debug.Log(claimData.game_id + " " + claimData.user_id + " " + claimData.claim_name);

        server.SendRequest(claimData , url);
        server.gotResponse += Response;
        
    }

    void Response(bool para)
    {
        Debug.Log(CommunicateWithServer.instance.responseData);
        CommunicateWithServer.instance.gotResponse -= Response;
    }

   
    void GameEnded(bool value)
    {
        Debug.Log("game ended" + value);
    }


    /*public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Result(NetworkScript.instance.roomName, true);

            //Invoke("LoadNextLevel", 5);
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            string c = (TournamentResult.instance.testNumber++).ToString();
            SendClaim(c);
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            Reward(200, int.Parse(PlayerPrefs.GetString("UserID")), "credit");
        }
    }*/

    public void Result(string gameId , bool isFinal)
    {
        if (!resultShown)
        {
            //StartCoroutine(ResultRoutine(gameId , isFinal));
            var url = StaticDetatils.baseUrl + "Api/Claim/read.php";
            ResultParamter p = new ResultParamter()
            {
                game_id = gameId
            };
            
            server.SendRequest(p, url);
            server.gotResponse += ResultCallback;
        }
    }

    void ResultCallback(bool para)
    {

            HashSet<string> tournamentResult = new HashSet<string>();
            string json = server.responseData;
            Debug.Log("Final result " + json);

            FinalResultResponse res = JsonUtility.FromJson<FinalResultResponse>(json);
            //if(res.status == "200")
            //{
            for (int i = 0; i < res.items.Count; i++)
            {
                string s = JsonUtility.ToJson(res.items[i]);
                Debug.Log(" res " + s);
                ResultData dat = JsonUtility.FromJson<ResultData>(s);
                string claimString = "";
                if (dat.claim_name.Count > 1)
                    dat.claim_name.Remove("None");
                foreach (string c in dat.claim_name)
                {
                    claimString += c + ",";
                    claimedList.Add(c);
                }
                claimString = claimString.Remove(claimString.Length - 1);
                tournamentResult.Add(s);
                if (SceneManager.GetActiveScene().name == "Tournament1" || SceneManager.GetActiveScene().name == "Tournament2")
                {

                }
                else
                {
                    GameObject g = Instantiate(resultPrefab, resultContent.transform);
                    g.transform.Find("name").GetComponent<Text>().text = GetUser(dat.user_id) + dat.user_id.ToString();
                    g.transform.Find("claims").GetComponent<Text>().text = claimString;
                    claimedList.Clear();
                }

            }

            if (SceneManager.GetActiveScene().name == "Tournament1" || SceneManager.GetActiveScene().name == "Tournament2")
            {
                CommunicateWithServer.instance.responseData = "";
                Invoke("LoadNextLevel", 5);
            }
            else
            {
                if (SceneManager.GetActiveScene().name == "Tournament3")
                {
                    Dictionary<int, int> temp = new Dictionary<int, int>();
                    foreach (string s in tournamentResult)
                    {
                        ResultData dat = JsonUtility.FromJson<ResultData>(s);
                        TournamentResult.instance.result.Add(dat.user_id, dat.claim_name);
                        temp.Add(dat.user_id, dat.claim_name.Count);
                    }

                    var v = temp.OrderByDescending(key => key.Value);
                    List<int> rank_id = new List<int>();
                    foreach (KeyValuePair<int, int> d in v)
                    {
                        rank_id.Add(d.Key);
                    }

                    //1st
                    int amountFirst = TournamentResult.instance.pincipleAmount / 4;
                    Reward(amountFirst, rank_id[0], "credit");
                    //
                    //2nd
                    int amountSecond = TournamentResult.instance.pincipleAmount / 5;
                    Reward(amountSecond, rank_id[1], "credit");
                    //
                    //3rd
                    //int amountThird = (TournamentResult.instance.pincipleAmount / 20) * 3;
                    //Reward(amountThird, rank_id[2], "credit");
                    ////
                    ////4th
                    //int amountFourth = TournamentResult.instance.pincipleAmount / 10;
                    //Reward(amountFourth, rank_id[3], "credit");
                    ////
                    ////5th
                    //Reward(amountFourth, rank_id[4], "credit");

                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        string id = new string(PhotonNetwork.LocalPlayer.NickName.Where(Char.IsDigit).ToArray());
                        Reward(TournamentResult.instance.pincipleAmount / 5, int.Parse(id), "credit");
                    }
                }
                resultPanel.SetActive(true);
                resultShown = true;
                TournamentResult.instance.result.Clear();
                CommunicateWithServer.instance.responseData = "";
            resultShown = true;
        
           // }
            //CommunicateWithServer.instance.gotResponse -= ResultCallback;
        }
        
    }

    void Reward(int amount , int id , string utype)
    {
        var url = StaticDetatils.baseUrl + "Api/Users/wallet.php";
        Wallet wallet = new Wallet()
        {
            user_id = id,
            chip_amount = amount,
            utype = utype
        };
        Debug.Log("sending reward data " + JsonUtility.ToJson(wallet));
        server.SendRequest(wallet, url);
        server.gotResponse += ResponseWallet;
    }
  
    void ResponseWallet(bool para)
    {
        Debug.Log("Wallet Response " + CommunicateWithServer.instance.responseData);
    }

    string GetUser(int userId)
    {

        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            if(playerInfo.Value.NickName.Contains(userId.ToString()))
            {
                return playerInfo.Value.NickName;
            }
        }
        return null;
    }



    private void CreateTicketTest(int numberOfTickets)
    {

        List<int[,]> ticketMatrixList = TicketGenerator.instance.GenerateTheTicket();
        for(int i = 0; i < ticketMatrixList.Count; i++)
        {
            for (int r = 0; r < 3; r++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if(ticketMatrixList[i][r , col] == 0)
                    {
                        Transform t = Instantiate(EmptyCell);
                        t.SetParent(allTickets[i].ticket);
                        t.GetComponent<RectTransform>().localScale = Vector3.one;
                        if (r == 0)
                            allTickets[i].row1[col] = t;
                        else if (r == 1)
                            allTickets[i].row2[col] = t;
                        else if (r == 2)
                            allTickets[i].row3[col] = t;
                    }
                    else
                    {
                        Transform t = Instantiate(NumberCell);
                        t.GetChild(0).GetComponent<Text>().text = ticketMatrixList[i][r, col].ToString();
                        t.SetParent(allTickets[i].ticket);
                        t.GetComponent<RectTransform>().localScale = Vector3.one;

                        if (r == 0)
                            allTickets[i].row1[col] = t;
                        else if (r == 1)
                            allTickets[i].row2[col] = t;
                        else if (r == 2)
                            allTickets[i].row3[col] = t;

                    }
                }
            }
            RowNumberCollection(i);
        }

        for(int i = 0; i < Test.instance.numberOfTicket;i++)
        {
            Sorting(i);
        }


        //int[,] ticketMatrix = new int[3 , 9];
        
       /* List<int[,]> ticketMatrixList = new List<int[,]>();
        List<bool[,]> ticketMatrixFilledList = new List<bool[,]>();
        List<Transform[,]> ticketMatrixTransList = new List<Transform[,]>();
        for(int i = 0; i < numberOfTickets; i++)
        {
            ticketMatrixList.Add(new int[3, 9]);
            ticketMatrixFilledList.Add(new bool[3, 9]);
            ticketMatrixTransList.Add(new Transform[3, 9]);
        }
                                
        

        CreateTypeDictionary();
        for (int i = 0; i < ticketMatrixList.Count; i++)
        {
            for(int m = 0; m < 3; m++)
            {
                for(int n = 0; n < 9; n++)
                {
                    ticketMatrixList[i][m, n] = -1;
                    ticketMatrixFilledList[i][m, n] = false;
                    ticketMatrixTransList[i][m, n] = null;
                }
            }
        }

        #region  First Iteration
        for (int i = 0; i < ticketMatrixList.Count; i++)
        {
            List<int> columnIndexAvailable = new List<int>() { 0, 1, 2 };
            
            for (int m = 0; m < 9; m++)
            {
                List<Transform> arranger = new List<Transform>() { null, null, null };
                int r = columnIndexAvailable[UnityEngine.Random.Range(0, columnIndexAvailable.Count)];
                columnIndexAvailable.Remove(r);
                int randomNumberColumn = mainSplitHolder[m][UnityEngine.Random.Range(0, mainSplitHolder[m].Count)];
                ticketMatrixList[i][r,m] = randomNumberColumn;

                ticketMatrixFilledList[i][r, m] = true;

                mainSplitHolder[m].Remove(randomNumberColumn);
                Transform g = Instantiate(NumberCell);
                g.GetChild(0).GetComponent<Text>().text = randomNumberColumn.ToString();
                arranger[r] = g;
                
                for(int a = 0; a < 3; a++)
                {
                    if(arranger[a] == null)
                    {
                        Transform empty = Instantiate(NumberCell);
                        empty.GetChild(0).GetComponent<Text>().text = "";
                        arranger[a] = empty;
                    }
                }

                arranger[0].SetParent(allTestTickets[i].ticket);
                arranger[1].SetParent(allTestTickets[i].ticket);
                arranger[2].SetParent(allTestTickets[i].ticket);

                ticketMatrixTransList[i][0,m] = arranger[0];
                ticketMatrixTransList[i][1,m] = arranger[1];
                ticketMatrixTransList[i][2,m] = arranger[2];

                arranger = new List<Transform>() { null, null, null };
    
                if (columnIndexAvailable.Count <= 0)
                    columnIndexAvailable = new List<int>() { 0, 1, 2 };
            }
        }
        #endregion

        List<int> leftNumberInSet = new List<int>() {4 , 4, 4 , 4 ,4 , 4 ,4 , 4 , 4 };
        List<int> leftNumberCopy = leftNumberInSet;

        #region Second Iteration
        
        for(int i = 0; i < numberOfTickets; i++)
        {
            leftNumberCopy.Sort();
            for(int m = 0; m < 3; m++)
            {
                List<int> availableNumber = new List<int>();
                for (int k = 0; k < 9; k++)
                    if (ticketMatrixFilledList[i][0, k] == false)
                        availableNumber.Add(k);

                int r = availableNumber[UnityEngine.Random.Range(0, availableNumber.Count)];
                leftNumberInSet[r] -= 1;
                int randomNumberColumn = UnityEngine.Random.Range(r * 10, (r * 10) + 10);
                ticketMatrixList[i][m, r] = randomNumberColumn;
                Debug.Log("index : " + r + " Number " + randomNumberColumn);

                Transform trans = ticketMatrixTransList[i][m, r];
                trans.GetChild(0).GetComponent<Text>().text = randomNumberColumn.ToString();

            }
        }

        #endregion

        #region Third Iteration
        #endregion*/
    }

    void SetVariableUsingName(string variableName, bool value)
    {
        
        Debug.Log("************ after changed " + this.GetType().GetField(variableName).GetValue(this));
    }

}

