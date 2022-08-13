using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class TutorialVideoManager : MonoBehaviour
{
    [SerializeField] VideoPlayer vp;
    [SerializeField] VideoClip tutorial, gameplay;
    public void SetVideoToTutorial()
    {
        vp.clip = tutorial;
        vp.Play();
    }

    public void SetVideoToGameplay()
    {
        vp.clip = gameplay;
        vp.Play();
    }
}
