using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class RealtimeDatabase : MonoBehaviour
{
    DatabaseReference reference;

    public class UserDetails
    {
        public string userID;
        public string userName;
        public int userCoins;
        public int userTickets;
        public int userProfilePicIndex;
        public bool dataSaved;
    }
    void Start()
    {
        // reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveData()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        UserDetails userDetails = DataToBeUploaded();

        string json = JsonUtility.ToJson(userDetails);
        reference.Child(PlayerPrefs.GetString("UserID")).Child(PlayerPrefs.GetString("UserLoginInfo")).SetRawJsonValueAsync(json).ContinueWith(task =>{
            if(task.IsCompleted)
                Debug.Log("Successfully Uploaded");
            else
                Debug.Log("Failed Uploading");
        });
    }

    public bool ReadData(string ID, string loginInfo)
    {
        bool result = false;
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        reference.Child(ID).Child(loginInfo).GetValueAsync().ContinueWith(task =>{
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                Debug.Log("Compeleted");
                PlayerPrefs.SetString("UserID", snapshot.Child("userID").Value.ToString());
                PlayerPrefs.SetString("UserName", snapshot.Child("userName").Value.ToString());
                PlayerPrefs.SetInt("UserCoin", (int)snapshot.Child("userCoins").Value);
                PlayerPrefs.SetInt("UserProfilePicIndex", (int)snapshot.Child("userProfilePicIndex").Value);
                PlayerPrefs.SetInt("UserTickets", (int)snapshot.Child("userTickets").Value);
                result = (bool)snapshot.Child("dataSaved").Value;
            }
        });

        return result;
    }

    UserDetails DataToBeUploaded()
    {
        UserDetails userDetails = new UserDetails();

        userDetails.userID = PlayerPrefs.GetString("UserID");
        userDetails.userName = PlayerPrefs.GetString("UserName");
        userDetails.userCoins = PlayerPrefs.GetInt("UserCoin");
        userDetails.userTickets = PlayerPrefs.GetInt("UserTickets");
        userDetails.userProfilePicIndex = PlayerPrefs.GetInt("UserProfilePicIndex");
        userDetails.dataSaved = true;

        return userDetails;
    }
}
