using UnityEngine;
using UnityEngine.UI;

public class GuestSignIn : MonoBehaviour
{

    [SerializeField] Text rankText;
    [SerializeField] InputField username;
    [SerializeField] InputField password;


    public void GuestLogIn()
    {
        string userId = SystemInfo.deviceUniqueIdentifier.Substring(0, 15);
        // bool result = realtimeDatabase.ReadData(userId, "Guest");
        // Debug.Log(result);
        
        PlayerPrefs.SetString("UserLoginInfo", "Guest");
        if(!PlayerPrefs.HasKey("UserEmail"))
        {
            PlayerPrefs.SetString("UserEmail", "Guest" + Random.Range(0, 9999999).ToString() + "@nmsgames.com");
            PlayerPrefs.SetInt("UserID", 0);
        }
        PlayerPrefs.SetString("UserPassword", userId);
        PlayerPrefs.SetString("UserName", "Guest");
        PlayerPrefs.SetInt("UserTickets", 0);
        PlayerPrefs.SetInt("UserProfilePicIndex", 2);
        PlayerPrefs.SetInt("UserCoin", 500);
        PlayerPrefs.SetString("DeviceID", SystemInfo.deviceUniqueIdentifier);

        LoginScript.instance.SignIn();
        rankText.text = PlayerPrefs.GetString("Rank");
    }

    void Login()
    {
        PlayerPrefs.SetString("UserLoginInfo", "Guest");
        PlayerPrefs.SetString("Login", "Yes");
    }

    public void LoginUsingUsernameAndPassword()
    {
        LoginScript.instance.SignInWithCredentials(username.text, password.text);
    }
}
