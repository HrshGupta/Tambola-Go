using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ProfileEditPanel : MonoBehaviour
{
    [SerializeField] RealtimeDatabase realtimeDatabase;
    public Transform[] dps;
    public Image[] dpThatWillChange;
    GameObject dp;
    [SerializeField] AudioSource clickAudio;

    void Awake()
    {
        DeSelectAll();
    }

    void DeSelectAll()
    {
        foreach(Transform item in dps)
            item.GetChild(0).gameObject.SetActive(false);
    }

    public void Selected(GameObject selected)
    {
        DeSelectAll();
        selected.SetActive(true);
        dp = selected;
    }

    public void OnClickProfileOkButton()
    {
        clickAudio.Play();

        foreach (var item in dpThatWillChange)
        {
            item.sprite = dp.transform.parent.GetComponent<Image>().sprite;
            PlayerPrefs.SetInt("UserProfilePicIndex", Int32.Parse(dp.transform.parent.GetComponent<Image>().sprite.name));
            // gameObject.SetActive(false);
        }
        realtimeDatabase.SaveData();
    }

    public void OnClickBackButton()
    {
        clickAudio.Play();
        gameObject.SetActive(false);
    }
}
