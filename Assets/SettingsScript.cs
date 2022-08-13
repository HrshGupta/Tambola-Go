using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SettingsScript : MonoBehaviour
{
    [SerializeField] RealtimeDatabase realtimeDatabase;
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] Slider brightness;
    [SerializeField] Image brightnessPanel;
    [SerializeField] AudioSource clickAudio;
    [SerializeField] SoundScript soundScript;

    public void OnClickClose()
    {
        clickAudio.Play();
        realtimeDatabase.SaveData();
        gameObject.SetActive(false);
    }

    public void OnClickDecreament()
    {
        clickAudio.Play();
        float bright = brightness.value;
        if(bright < 0.5f)
            brightness.value = bright + 0.05f;        
    }

    public void OnClickIncreament()
    {
        clickAudio.Play();
        float bright = brightness.value;
        if(bright > 0f)
            brightness.value = bright - 0.05f;       
    }

    public void OnClickLogout()
    {
        clickAudio.Play();
        
        PlayerPrefs.SetString("Login", "No");
        
        if(PlayerPrefs.GetString("UserLoginInfo") == "Google")
            loginPanel.GetComponent<GoogleSignInDemo>().SignOutFromGoogle();
        
        if(PlayerPrefs.GetString("UserLoginInfo") == "FaceBook")
            loginPanel.GetComponent<FacebookAuth>().FacebookLogout();

        PlayerPrefs.DeleteAll();

        soundScript.Save();

        loginPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void BrightnessValueChanged()
    {
        PlayerPrefs.SetFloat("BrightnessValue", brightness.value);
        brightnessPanel.color = new Color(0, 0, 0, brightness.value);
    }
}
