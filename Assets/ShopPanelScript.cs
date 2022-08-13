using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelScript : MonoBehaviour
{
    [SerializeField] AudioSource clickAudio;
    public void OnClickClose()
    {
        clickAudio.Play();
        gameObject.SetActive(false);
    }
}
