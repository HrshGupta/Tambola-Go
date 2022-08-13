using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class FacebookAuth : MonoBehaviour
{
    [SerializeField] GameObject lobbyPanel;
    public Image[] dpThatWillChange;
    [SerializeField] RealtimeDatabase realtimeDatabase;

    string token;
    private void Awake()
    {
        if(!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                if(FB.IsInitialized)
                {
                    FB.ActivateApp();
                    Debug.Log("");
                }


            },
            isGameShown =>
            {
                if(!isGameShown)
                    Time.timeScale = 0;
                else
                    Time.timeScale = 1;
            });
        }
        else
            FB.ActivateApp();
    }

    // #region Login / Logout
    public void FacebookLogin()
    {
        var permissions = new List<string>() { "public_profile", "email", "user_friends" };
        FB.LogInWithReadPermissions(permissions, AuthCallBack);
    }

    private void AuthCallBack(ILoginResult result)
    {
        
        if(FB.IsLoggedIn)
        {
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // PlayerPrefs.SetString("UserID", "0");
            //PlayerPrefs.GetString("UserEmail");
            PlayerPrefs.SetString("UserLoginInfo", "Facebook");
            PlayerPrefs.SetString("UserPassword", aToken.UserId);
            FB.API("/me?fields=name, email", HttpMethod.GET, DisplayUserName);
            // FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayUserProfile);
            

        }
        else
        {
             Debug.Log("User Cancelled Login");
        }
    }

    public void FacebookLogout()
    {
        if(PlayerPrefs.HasKey("UserLoginInfo"))
        {
            if(PlayerPrefs.GetString("UserLoginInfo") == "FaceBook")
            {
                if(FB.IsLoggedIn)
                    FB.LogOut();
                // uiManager.DelSaveData();
            }
        }
    }

    void DisplayUserName(IResult result)
    {
        if(result.Error == null)
        {
            Debug.Log(result.RawResult);
            foreach(KeyValuePair<string , object> p in result.ResultDictionary)
            {
                Debug.Log("key: " + p.Key + " value: " + p.Value);
            }

            string name = result.ResultDictionary["name"].ToString(); //+ result.ResultDictionary["last_name"].ToString();
            string password = result.ResultDictionary["id"].ToString();
            PlayerPrefs.SetString("UserName", name);
            PlayerPrefs.SetString("UserEmail", result.ResultDictionary["email"].ToString());
            //StartCoroutine(Login(token, name, result.ResultDictionary["email"].ToString()));
            //

            PlayerPrefs.SetInt("UserProfilePicIndex", 2);
            //PlayerPrefs.SetInt("UserTickets", 0);
            //PlayerPrefs.SetInt("UserCoin", 1000);
            PlayerPrefs.SetString("DeviceID", SystemInfo.deviceUniqueIdentifier);
            //PlayerPrefs.SetString("UserEmail", "Guest" + Random.Range(0, 9999999).ToString() + "@nmsgames.com");
            //token = aToken.TokenString;
            PlayerPrefs.SetString("UserName", name);
            PlayerPrefs.SetString("UserPassword", password);
            LoginScript.instance.SignIn();

        }
        else
        {
            Debug.Log(result.Error);
        }
    }

    void DisplayUserProfile(IGraphResult result)
    {
        if(result.Texture != null)
        {
            foreach (var item in dpThatWillChange)
            {
                item.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
            }
        }
    }

    public void InviteFriends()
    {
        FB.AppRequest("Play tambola in this room " + Test.instance.roomName);
    }


    IEnumerator Login(string token, string user, string mail)
    {
        var url = StaticDetatils.baseUrl + "Api/Users/socialLogin.php";
        //var url = StaticDetatils.baseUrl + "Api/Users/userLogin.php";

        SocialLogin login = new SocialLogin()
        {
            google_id = token,
            device_id = SystemInfo.deviceUniqueIdentifier,
            username = user,
            email = mail
        };

        string data = JsonUtility.ToJson(login);
        Debug.Log(data);


        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;

            Debug.Log("Login Data " + json);

            Response res = JsonUtility.FromJson<Response>(json);
            SocialLoginData d = JsonUtility.FromJson<SocialLoginData>(JsonUtility.ToJson(res.data));
            
            if(res.status == "200")
            {

                lobbyPanel.SetActive(true);
            }
            //LoggedIn info = JsonUtility.FromJson<LoggedIn>(json);
            //
            //Debug.Log(json);
            //AndroidToastMsg.ShowAndroidToastMessage(info.message);
            //
            //PlayerPrefs.SetString("UserEmail", info.data.email);
            //PlayerPrefs.SetString("UserName", info.data.username);
            //PlayerPrefs.SetString("UserID", info.data.user_id.ToString());
        }
    }
    // #endregion
}

[System.Serializable]
public class SocialLogin
{
    public string google_id;
    public string device_id;
    public string username;
    public string email;
}


[System.Serializable]
public class SocialLoginData
{
    public string userId;
    public string profileId;
    public string deviceId;
    public string email;
    public string totalCoins;
    public string username;
    public string avatarLink;
    public string playerRank;
}

[System.Serializable]
public class Response
{
    public string status;
    public string message;
    public SocialLoginData data;
}