using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundScript : MonoBehaviour
{
    [SerializeField] AudioSource sound;
    [SerializeField] GameObject[] onOff;
    private bool muted = false;

    // Start is called before the first frame update
    void Start()
    {
        if(transform.name == "Music")
        {
            if(!PlayerPrefs.HasKey("Music"))
            {
                PlayerPrefs.SetInt("Music", 0);
                Load();
            }
            else
            {
                Load();
            }   
        }

        if(transform.name == "Sound")
        {
            if(!PlayerPrefs.HasKey("Sound"))
            {
                PlayerPrefs.SetInt("Sound", 0);
                Load();
            }
            else
            {
                Load();
            }   
        }

        sound.enabled = !muted;
    }

    public void OnButtonPress()
    {
        if(!muted)
        {
            muted = true;
            sound.enabled = false;
            onOff[0].SetActive(false);
            onOff[1].SetActive(true);
        }
        else
        {
            muted = false;
            sound.enabled = true;
            onOff[0].SetActive(true);
            onOff[1].SetActive(false);
        }

        Save();
    }

    private void Load()
    {
        if(transform.name == "Music")
            muted = PlayerPrefs.GetInt("Music") == 1;

        if(transform.name == "Sound")
            muted = PlayerPrefs.GetInt("Sound") == 1;

        if(SceneManager.GetActiveScene().name == "Login")
        {
            onOff[0].SetActive(!muted);
            onOff[1].SetActive(muted);
        }
    }

    public void Save()
    {
        if(transform.name == "Music")
            PlayerPrefs.SetInt("Music", muted ? 1 : 0);

        if(transform.name == "Sound")
            PlayerPrefs.SetInt("Sound", muted ? 1 : 0);
    }
}
