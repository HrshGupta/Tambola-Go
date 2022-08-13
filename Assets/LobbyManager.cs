using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    [Header("User Datils To Be Set")]
    [SerializeField] Text[] userNames;
    [SerializeField] Text[] userIds;
    public Text[] userCoins;
    [SerializeField] Text[] userTickets;
    [SerializeField] Image[] UserProfilePic;
    [SerializeField] Text rank;

    [Header("Data")]
    [SerializeField] Sprite[] dataProfiles;
    [SerializeField] Slider brightness;
    [SerializeField] RealtimeDatabase realtimeDatabase;
    [SerializeField] NetworkScript networkScript;

    [Header("Panels")]
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject ProfilePanel;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject howToPlayPanel;
    [SerializeField] GameObject regularProPanel;
    [SerializeField] GameObject regularPanel;
    [SerializeField] GameObject lazzyPanel;
    [SerializeField] GameObject quickPanel;
    [SerializeField] GameObject rapidPanel;
    [SerializeField] GameObject normalPanel;
    [SerializeField] GameObject tournamentPanel;
    [SerializeField] GameObject ticketBuyPanel;
    [SerializeField] GameObject proPanel;
    [SerializeField] GameObject normalSlipGamesPanel;
    [SerializeField] GameObject sixSlipGamesPanel;
    [SerializeField] GameObject practicePanel;

    [SerializeField] GameObject categoryPanel;
    [SerializeField] GameObject subCategoryPanel;
    [SerializeField] GameObject createOrJoinRoomPanel;
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] GameObject joinRoomPanel;
    [SerializeField] GameObject waittingPanel;
    [SerializeField] Image brightnessPanel;
    [SerializeField] InputField username;
    [SerializeField] GameObject setuserPanel;
    [SerializeField] GameObject waitingTimergb;

    [Header("Audios")]
    [SerializeField] AudioSource clickAudio;
    [SerializeField] AudioSource musicAudio;
    [Header("Claim Info"), Space(10)]
    [SerializeField] GameObject claimsContent;
    [SerializeField] GameObject claimDes;
    [SerializeField] GameObject claimInfoTicket;
    [SerializeField] InputField searchButtonIF;
    public CommunicateWithServer server;


    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        brightness.value = PlayerPrefs.GetFloat("BrightnessValue");
        brightnessPanel.color = new Color(0, 0, 0, brightness.value);
        loginPanel.SetActive(false);
        shopPanel.SetActive(false);
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        regularProPanel.SetActive(false);
        regularPanel.SetActive(false);
        lazzyPanel.SetActive(false);
        quickPanel.SetActive(false);
        rapidPanel.SetActive(false);
        normalPanel.SetActive(false);
        tournamentPanel.SetActive(false);
        ticketBuyPanel.SetActive(false);
        proPanel.SetActive(false);
        normalSlipGamesPanel.SetActive(false);
        sixSlipGamesPanel.SetActive(false);
        categoryPanel.SetActive(false);
        subCategoryPanel.SetActive(false);
        createOrJoinRoomPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        joinRoomPanel.SetActive(false);
        waittingPanel.SetActive(false);
        UserDataSet();
        // Invoke("UploadDataOnFirebase", 2);

        ClaimManager.instance.claimDescription = claimDes;
        ClaimManager.instance.GenerateClaims(claimsContent.transform);
        ClaimManager.instance.ticket = claimInfoTicket;
        searchButtonIF.onValueChanged.AddListener(delegate { ClaimManager.instance.Search(searchButtonIF); });


    }

    public void UploadDataOnFirebase()
    {
        realtimeDatabase.enabled = true;
        realtimeDatabase.SaveData();
    }

    public void UserDataSet()
    {
        foreach(Text item in userNames)
            item.text = PlayerPrefs.GetString("UserName");
        
        foreach(Text item in userIds)
            item.text = PlayerPrefs.GetString("UserID");
        
        foreach(Text item in userCoins)
        {
            if (PlayerPrefs.GetInt("UserCoin") >= 0)
                item.text = PlayerPrefs.GetInt("UserCoin").ToString();
            else
            {
                PlayerPrefs.SetInt("UserCoin", 0);
                item.text = "0";
            }
                
        }
            
        
        foreach(Text item in userTickets)
            item.text = PlayerPrefs.GetInt("UserTickets").ToString();
        
        foreach(Image item in UserProfilePic)
            item.sprite = dataProfiles[PlayerPrefs.GetInt("UserProfilePicIndex")];

        rank.text = PlayerPrefs.GetString("Rank");
        PhotonNetwork.NickName = PlayerPrefs.GetString("UserName") + "\n" + PlayerPrefs.GetString("UserID");
    }

    // public void OnClickPlayButton()
    // {
    //     SceneManager.LoadScene("GamePlay");
    // }
    

    public void OnClickShopButton()
    {
        clickAudio.Play();
        shopPanel.SetActive(true);
    }

    public void OnClickSettingsButton()
    {
        clickAudio.Play();
        settingsPanel.SetActive(true);
    }

    public void OnClickHowToPlayButton()
    {
        clickAudio.Play();
        howToPlayPanel.SetActive(true);
        

    }

    public void HowToPlayBackButton()
    {
        clickAudio.Play();
        howToPlayPanel.SetActive(false);
    }

    public void OnClickProfileButton()
    {
        clickAudio.Play();
        ProfilePanel.SetActive(true);
    }

    public void OnClickPlayButton()
    {
        clickAudio.Play();
        regularProPanel.SetActive(true);
    }

    public void OnClickRegularProBackButton()
    {
        clickAudio.Play();
        regularProPanel.SetActive(false);
    }

    public void OnClickRegularButton()
    {
        clickAudio.Play();
        regularPanel.SetActive(true);
    }

    public void OnClickRegularBackButton()
    {
        clickAudio.Play();
        regularPanel.SetActive(false);
    }

    public void OnClickLazzyButton()
    {
        clickAudio.Play();
        Test.instance.currentGameName = "Lazzy";
        lazzyPanel.SetActive(true);
    }

    public void OnClickLazzyBackButton()
    {
        clickAudio.Play();
        lazzyPanel.SetActive(false);
    }

    public void OnClickQuickButton()
    {
        clickAudio.Play();
        Test.instance.currentGameName = "Quick";
        quickPanel.SetActive(true);
    }

    public void OnClickQuickBackButton()
    {
        clickAudio.Play();
        quickPanel.SetActive(false);
    }

    public void OnClickRapidButton()
    {
        clickAudio.Play();
        Test.instance.currentGameName = "Rapid";
        rapidPanel.SetActive(true);
    }

    public void OnClickRapidBackButton()
    {
        clickAudio.Play();
        rapidPanel.SetActive(false);
    }

    public void OnClickNormalButton()
    {
        clickAudio.Play();
        Test.instance.currentGameName = "Normal";
        normalPanel.SetActive(true);
    }

    public void OnClickNormalBackButton()
    {
        clickAudio.Play();
        normalPanel.SetActive(false);
    }

    public void OnClickTournamentButton()
    {
        clickAudio.Play();
        Test.instance.currentGameName = "Tournament1";
        tournamentPanel.SetActive(true);
    }

    public void OnClickTournamentBackButton()
    {
        clickAudio.Play();
        
        tournamentPanel.SetActive(false);
    }

    public void OnClickTicketBuyButton()
    {
        clickAudio.Play();
        ticketBuyPanel.SetActive(true);
    }

    public void OnClickTicketBuyBackButton()
    {
        clickAudio.Play();
        ticketBuyPanel.SetActive(false);
    }

    public void OnClickProButton()
    {
        proPanel.SetActive(true);
    }

    public void OnClickProBackButton()
    {
        proPanel.SetActive(false);
    }

    public void OnClickNormalSlipGamesButton()
    {
        Test.instance.currentGameName = "Normal Slip Games";
        normalSlipGamesPanel.SetActive(true);
    }

    public void OnClickNormalSlipGamesBackButton()
    {
        normalSlipGamesPanel.SetActive(false);
    }

    public void OnClickSixSlipGamesButton()
    {
        Test.instance.currentGameName = "Six Slip Games";
        Test.instance.numberOfTicket = 6;
        sixSlipGamesPanel.SetActive(true);
    }

    public void OnClickPracticeButton()
    {
        Test.instance.currentGameName = "Practice";
        Test.instance.numberOfTicket = 1;
        practicePanel.SetActive(true);
    }
    public void OnClickPracticeBackButton()
    {
        ;
        clickAudio.Play();
        practicePanel.SetActive(false);
    }

    public void OnClickSixSlipGamesBackButton()
    {
        sixSlipGamesPanel.SetActive(false);
    }

    public void OnClickCategoryButton()
    {
        categoryPanel.SetActive(true);
    }

    public void OnClickCategoryBackButton()
    {
        categoryPanel.SetActive(false);
    }

    public void OnClickSubCategoryButton()
    {
        subCategoryPanel.SetActive(true);
    }

    public void OnClickSubCategoryBackButton()
    {
        subCategoryPanel.SetActive(false);
    }

    public void OnClickCreateOrJoinRoomButton(string roomType)
    {
        networkScript.gameType = roomType;
        createOrJoinRoomPanel.SetActive(true);
    }

    public void OnClickCreateOrJoinRoomBackButton()
    {
        createOrJoinRoomPanel.SetActive(false);
    }

    public void OnClickCreateRoomButton()
    {
        createRoomPanel.SetActive(true);
    }

    public void OnClickCreateRoomBackButton()
    {
        createRoomPanel.SetActive(false);
    }

    public void OnClickJoinRoomButton()
    {
        joinRoomPanel.SetActive(true);
    }

    public void OnClickJoinRoomBackButton()
    {
        joinRoomPanel.SetActive(false);
    }

    public void OnClickEnterButton()
    {
        waittingPanel.SetActive(true);
 
    }

    public void OnClickWaittingBackButton()
    {
        PhotonNetwork.LeaveRoom();
        networkScript.StopAllCoroutines();
        Test.instance.numberOfTicket = 1;
        waitingTimergb.SetActive(false);
        waittingPanel.SetActive(false);
    }

    public void OnClickOpenSocialPage()
    {
        Application.OpenURL("http://165.22.208.86/Index.html");
    }

    public void OnClickSubmitusername()
    {
        PlayerPrefs.SetString("UserName", username.text);
        setuserPanel.SetActive(false);
        PlayerPrefs.SetString("UsernameSet", "yes");

        Debug.Log(PlayerPrefs.GetString("UserName"));
        UserDataSet();
        
    }

    public void ShareRoom()
    {
        new NativeShare().SetText("Hey! Play Ult Pult Tambola with me in this room " + Test.instance.roomName).Share();
    }

}
