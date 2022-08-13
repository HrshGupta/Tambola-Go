using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleScript : MonoBehaviour
{
    [SerializeField] GameObject offObj;
    [SerializeField] GameObject onObj;
    [SerializeField] RectTransform uiHandleRectTransform;
    Toggle toggle;
    Vector2 handlePosition;
    [SerializeField] AudioSource clickAudio;
    [SerializeField] AudioSource music;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        handlePosition = uiHandleRectTransform.anchoredPosition;
        toggle.onValueChanged.AddListener(OnSwitch);

        if(transform.name == "MusicToggle")
        {
            if(PlayerPrefs.GetInt("Music") == 1)
            {
                toggle.isOn = true;
                music.mute = false;
                OnSwitch(true);
            }

            if(PlayerPrefs.GetInt("Music") == 0)
            {
                toggle.isOn = false;
                music.mute = true;
                OnSwitch(false);
            }
            
        }

        if(transform.name == "SoundToggle")
        {
            if(PlayerPrefs.GetInt("Sound") == 1)
            {
                toggle.isOn = true;
                clickAudio.mute = false;
                OnSwitch(true);
            }

            if(PlayerPrefs.GetInt("Sound") == 0)
            {
                toggle.isOn = false;
                clickAudio.mute = true;
                OnSwitch(false);
            }
            
        }

        // onObj.SetActive(!toggle.isOn);
        // offObj.SetActive(toggle.isOn);

        // if(toggle.isOn)
        //     OnSwitch(true);
        // else
        //     OnSwitch(false);
    }

    void OnSwitch(bool on)
    {
        clickAudio.Play();
        uiHandleRectTransform.anchoredPosition = on ? new Vector2(handlePosition.x + 60, handlePosition.y) : new Vector2(handlePosition.x - 60, handlePosition.y);
        switch(transform.name)
        {
            case "MusicToggle":
                music.mute = !toggle.isOn;

                if(!toggle.isOn == true)
                    PlayerPrefs.SetInt("Music", 1);
                else
                    PlayerPrefs.SetInt("Music", 0);
                break;
                
            case "SoundToggle":
                clickAudio.mute = !toggle.isOn;

                if(!toggle.isOn == true)
                    PlayerPrefs.SetInt("Sound", 1);
                else
                    PlayerPrefs.SetInt("Sound", 0);
                break;
        }

        onObj.SetActive(toggle.isOn);
        offObj.SetActive(!toggle.isOn);
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnSwitch);
    }
}
