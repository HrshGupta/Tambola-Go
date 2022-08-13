using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using UnityEngine.Networking;

public class IAPShop : MonoBehaviour
{
    [SerializeField] RealtimeDatabase realtimeDatabase;
    [SerializeField] Text[] coins;

    public void OnPurchaseComplete(Product product)
    {
        float currentCoins = float.Parse(coins[0].text);
        if(product.definition.id == "com.nmsgames.tambola_project.10coin")
        {
            currentCoins = currentCoins + 10f;
            foreach(Text item in coins)
                item.text = currentCoins.ToString();

            StartCoroutine(PostPurchaseDataToServer(10));
            PlayerPrefs.SetString("UserCoin", currentCoins.ToString());
        }

        if(product.definition.id == "com.nmsgames.tambola_project.50coin")
        {
            currentCoins = currentCoins + 50f;
            foreach(Text item in coins)
                item.text = currentCoins.ToString();

            StartCoroutine(PostPurchaseDataToServer(50));
            PlayerPrefs.SetString("UserCoin", currentCoins.ToString());
        }

        if(product.definition.id == "com.nmsgames.tambola_project.100coin")
        {
            currentCoins = currentCoins + 100f;
            foreach(Text item in coins)
                item.text = currentCoins.ToString();

            StartCoroutine(PostPurchaseDataToServer(100));
            PlayerPrefs.SetString("UserCoin", currentCoins.ToString());
        }

        if(product.definition.id == "com.nmsgames.tambola_project.500coin")
        {
            currentCoins = currentCoins + 500f;
            foreach(Text item in coins)
                item.text = currentCoins.ToString();

            StartCoroutine(PostPurchaseDataToServer(500));
            PlayerPrefs.SetString("UserCoin", currentCoins.ToString());
            realtimeDatabase.SaveData();
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        // Debug.Log("Purchase of " + product.definition.id + " failed due to " + reason);
    }


    IEnumerator PostPurchaseDataToServer(int chipAmount)
    {
        var url = StaticDetatils.baseUrl + "Api/Users/buyChips.php";

        LogIn logIn = new LogIn();
        IAP iap = new IAP()
        {
            userId = PlayerPrefs.GetString("UserID"),
            chipAmount = chipAmount.ToString(),
            deviceId = SystemInfo.deviceUniqueIdentifier
        };


        string data = JsonUtility.ToJson(iap);
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
            LoggedIn info = JsonUtility.FromJson<LoggedIn>(json);

            Debug.Log(json);
        }
    }
}



[System.Serializable]
public class IAP
{
    public string userId;
    public string chipAmount;
    public string deviceId;
}