using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestHandler : MonoBehaviour
{
    public static WebRequestHandler Instance;
    private void Awake()
    {
        Instance = this;
    }
    public void Get(string url, Action<string, bool> OnRequestProcessed)
    {
        UnityEngine.Debug.Log("url::" + url);
        StartCoroutine(GetRequest(url, OnRequestProcessed));
    }

    public void Post(string url, List<IMultipartFormSection> form, Action<string, bool> OnRequestProcessed)
    {
        StartCoroutine(PostRequest(url, form, OnRequestProcessed));
    }

    public void Post(string url, string json, Action<string, bool> OnRequestProcessed)
    {
        StartCoroutine(PostRequest(url, json, OnRequestProcessed));
    }

    private IEnumerator GetRequest(string url, Action<string, bool> OnRequestProcessed)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("web request error in Get method with responce code : " + request.responseCode);
            OnRequestProcessed(request.error, false);
        }
        else
        {
            OnRequestProcessed(request.downloadHandler.text, true);
        }

        request.Dispose();
    }

    private IEnumerator PostRequest(string url, List<IMultipartFormSection> form, Action<string, bool> OnRequestProcessed)
    {

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            OnRequestProcessed(request.error, false);
        }
        else
        {
            OnRequestProcessed(request.downloadHandler.text, true);
        }
        request.Dispose();
    }

    private IEnumerator PostRequest(string url, string json, Action<string, bool> OnRequestProcessed)
    {
        Debug.Log("Sending api request for {" + url + "}: \n" + json);
        var request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            OnRequestProcessed(request.error, false);
            Debug.Log("Received error response for {" + url + "}: \n" + request.error);
        }
        else
        {
            OnRequestProcessed(request.downloadHandler.text, true);
            Debug.Log("Received successful response for {" + url + "}: \n" + request.downloadHandler.text);
        }
        request.Dispose();
    }

    public void DownloadImage(string url, Action<Sprite> OnDownloadComplete)
    {
        StartCoroutine(LoadFromWeb(url, OnDownloadComplete));
    }

    IEnumerator LoadFromWeb(string url, Action<Sprite> OnDownloadComplete)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError("failed to download image");
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1f);
            OnDownloadComplete(sprite);
        }
    }

    public static int GetVersionCode()
    {
        try
        {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageMngr = context.Call<AndroidJavaObject>("getPackageManager");
            string packageName = context.Call<string>("getPackageName");
            AndroidJavaObject packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<int>("versionCode");

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 2;
        }
    }
}