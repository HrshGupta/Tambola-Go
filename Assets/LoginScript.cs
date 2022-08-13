using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft;


public class LoginScript : MonoBehaviour
{

    public static LoginScript instance;

    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] Transform eventContent;
    [SerializeField] Transform eventTab;

    [SerializeField] Transform ticketImage;
    [SerializeField] Transform ticketContent;
    [SerializeField] Transform ticketDots;
    [SerializeField] Transform ticketDotsContent;
    [SerializeField] GameObject ticketPanel;
    [SerializeField] Text dateTime;
    [SerializeField] GameObject setuserPanel;

    void Start()
    {
        //PlayerPrefs.DeleteAll();
        
        instance = this;

        if(PlayerPrefs.HasKey("UserLoginInfo"))
        {
            StartCoroutine(LogInUser());
            // lobbyPanel.SetActive(true);
        }
    }

    public void SignIn()
    {
        //if (Application.isEditor)
        //    lobbyPanel.SetActive(true);
        //else
            StartCoroutine(CreateUser());
    }

    public void SignInWithCredentials(string username , string password)
    {
        PlayerPrefs.SetString("UserEmail", username);
        PlayerPrefs.SetString("UserPassword", password);
        StartCoroutine(LogInUser(username , password));
    }


    IEnumerator CreateUser()
    {
        //var url = StaticDetatils.baseUrl + "tambola/Api/Users/create.php";
        var url = StaticDetatils.baseUrl + "/Api/Users/create.php";

        UserCreation userCreation = new UserCreation();
        
        userCreation.email = PlayerPrefs.GetString("UserEmail");
        userCreation.password = PlayerPrefs.GetString("UserPassword");
        userCreation.username = PlayerPrefs.GetString("UserName");
        userCreation.deviceId = PlayerPrefs.GetString("DeviceID");
        
        string data = JsonUtility.ToJson(userCreation);
        Debug.Log(data);

        var request = new UnityWebRequest(url , "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
 
        yield return request.SendWebRequest();


        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            UserCreated info = JsonUtility.FromJson<UserCreated>(json);

            Debug.Log("Creation data " + json);
            DataRecieved recieved = JsonUtility.FromJson<DataRecieved>(json);
            Debug.Log("Status : "  + recieved.status);
            Debug.Log("Message : "  + recieved.message);

            //AndroidToastMsg.ShowAndroidToastMessage(info.message);

            StartCoroutine(LogInUser());
            // lobbyPanel.SetActive(true);
        }
    }

    IEnumerator LogInUser(string username = null, string password = null)
    {
        var url = StaticDetatils.baseUrl + "/Api/Users/login.php";
        //var url = StaticDetatils.baseUrl + "Api/Users/userLogin.php";
   

        LogIn logIn = new LogIn();
        if(username == null && password == null)
        {
            logIn.email = PlayerPrefs.GetString("UserEmail");
            logIn.password = PlayerPrefs.GetString("UserPassword");
        }
        else
        {
            logIn.email = username;
            logIn.password = password;
        }

        

        string data = JsonUtility.ToJson(logIn);
        Debug.Log(data);


        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
 
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            LoggedIn info = JsonUtility.FromJson<LoggedIn>(json);

            if (info.status == 200)
            {

                Debug.Log("Login Data " + json);


                AndroidToastMsg.ShowAndroidToastMessage(info.message);

                //if(!PlayerPrefs.HasKey("UserEmail"))
                //{
                PlayerPrefs.SetString("UserEmail", info.data.email);
                if (!PlayerPrefs.HasKey("UserName"))
                {
                    PlayerPrefs.SetString("UserName", info.data.username);
                }
                PlayerPrefs.SetInt("UserCoin", info.data.total_coins);
                PlayerPrefs.SetString("UserID", info.data.user_id.ToString());
                PlayerPrefs.SetString("Rank", info.data.player_rank.ToString());
                //}


                Debug.Log("Rank " + info.data.player_rank);

                if (info.events.Count != 0)
                {
                    if (eventContent.childCount != 0)
                    {
                        for (int i = 0; i < eventContent.childCount; i++)
                        {
                            Destroy(eventContent.GetChild(i).gameObject);
                        }
                    }
                    foreach (Event item in info.events)
                    {
                        Transform tab = Instantiate(eventTab, eventContent);
                        tab.Find("Event ID/ID").GetComponent<Text>().text = item.event_id.ToString();
                        tab.Find("User Name/Name").GetComponent<Text>().text = item.event_name.ToString();
                        tab.Find("Date Time/Date").GetComponent<Text>().text = item.event_date.ToString();
                        tab.Find("Date Time/Time").GetComponent<Text>().text = item.event_time.ToString();
                        if (item.event_status.ToString() == "Active")
                        {
                            tab.GetComponent<Button>().onClick.AddListener(() => EventJoinScript.instance.OnClickEvent(item.event_id));
                            tab.Find("ActiveORExpired").GetComponent<Text>().color = Color.green;
                            tab.Find("ActiveORExpired").GetComponent<Text>().text = item.event_status.ToString();
                        }
                        else
                        {
                            tab.Find("ActiveORExpired").GetComponent<Text>().color = Color.red;
                            tab.Find("ActiveORExpired").GetComponent<Text>().text = item.event_status.ToString();
                        }


                    }
                }

                lobbyPanel.SetActive(true);
                if (PlayerPrefs.GetString("UserLoginInfo") == "Guest" && PlayerPrefs.GetString("UsernameSet", "no") == "no")
                {
                    setuserPanel.SetActive(true);
                }
                else
                {
                    setuserPanel.SetActive(false);
                }

            }
            else
                AndroidToastMsg.ShowAndroidToastMessage(info.message);

        }
    }

    IEnumerator CheckUserEvent()
    {
        //var url = StaticDetatils.baseUrl + "tambola/Api/Users/login.php";
        var url = StaticDetatils.baseUrl + "Api/Users/userLogin.php";

        LogIn logIn = new LogIn();

        logIn.email = PlayerPrefs.GetString("UserEmail");
        logIn.password = PlayerPrefs.GetString("UserPassword");

        string data = JsonUtility.ToJson(logIn);
        Debug.Log(data);


        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
 
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            LoggedIn info = JsonUtility.FromJson<LoggedIn>(json);

            if(info.events.Count != 0)
            {
                if(eventContent.childCount != 0)
                {
                    for(int i = 0; i < eventContent.childCount; i++)
                    {
                        Destroy(eventContent.GetChild(i).gameObject);
                    }
                }
                foreach(Event item in info.events)
                {
                    Transform tab = Instantiate(eventTab, eventContent);
                    tab.Find("Event ID/ID").GetComponent<Text>().text = item.event_id.ToString();
                    tab.Find("User Name/Name").GetComponent<Text>().text = item.event_name.ToString();
                    tab.Find("Date Time/Date").GetComponent<Text>().text = item.event_date.ToString();
                    tab.Find("Date Time/Time").GetComponent<Text>().text = item.event_time.ToString();
                    if(item.event_status.ToString() == "Active")
                    {
                        tab.Find("ActiveORExpired").GetComponent<Text>().color = Color.green;
                        tab.Find("ActiveORExpired").GetComponent<Text>().text = item.event_status.ToString();
                    }
                    else
                    {
                        tab.Find("ActiveORExpired").GetComponent<Text>().color = Color.red;
                        tab.Find("ActiveORExpired").GetComponent<Text>().text = item.event_status.ToString();
                    }

                    tab.GetComponent<Button>().onClick.AddListener(() => EventJoinScript.instance.OnClickEvent(item.event_id));
                }
            }
        }
    }

    public void CheckEvents()
    {
        StartCoroutine(CheckUserEvent());
    }
}

[System.Serializable]
public class UserCreated
{
    public int status ;
    public string message ;
}

[System.Serializable]
public class UserCreation
{
    public string email ;
    public string password ;
    public string username ;
    public string deviceId;
}

[System.Serializable]
public class LogIn
{
    public string email ;
    public string password ;
}

[System.Serializable]
public class Data
{
    public int user_id ;
    public string email ;
    public int total_coins ;
    public string username ;
    public object avatar_link ;
    public int player_rank;
}

[System.Serializable]
public class Event
{
    public string event_status ;
    public string event_id ;
    public string event_name ;
    public string event_date ;
    public string event_time ;
    public int number_of_ticket ;
    public List<LoggedInTicket> tickets ;
}

[System.Serializable]
public class LoggedInTicket
{
    public int ticket_id ;
    public string is_ticket_status ;
    public string ticket_name ;
    public string ticket_image ;
}

[System.Serializable]
public class LoggedIn
{
    public int status ;
    public string message ;
    public Data data ;
    public List<Event> events;
}



[System.Serializable]
public class DataRecieved
{
    public int status;
    public string message;
}

[System.Serializable]
public class LoginSentData
{
    public string email;
    public string password;
    public string username;
    public string deviceId;
}