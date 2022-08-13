using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
public class CommunicateWithServer : MonoBehaviour
{
    public static CommunicateWithServer instance;
    public Action<bool> gotResponse;
    public string responseData;

    private void Awake()
    {
            instance = this;
        
    }

    public void SendRequest(object data , string url)
    {
        StartCoroutine(SendRequestRoutine(data , url));
    }

    IEnumerator SendRequestRoutine(object data , string url)
    {
        string jsonData = JsonUtility.ToJson(data);
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.Success)
        {
            if(request.responseCode == 200)
            {
                responseData = request.downloadHandler.text;
                Debug.Log(request.downloadHandler.text);
                gotResponse(request.downloadHandler.isDone);
                

            }
        }
    }
}

