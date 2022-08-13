using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class MovieController : MonoBehaviour
{
    private VideoPlayer m_VideoPlayer;

    void Awake () 
    {
        //PlayerPrefs.DeleteAll();
        m_VideoPlayer = GetComponent<VideoPlayer>();
        m_VideoPlayer.loopPointReached += OnMovieFinished; // loopPointReached is the event for the end of the video
    }

    void OnMovieFinished(VideoPlayer player)
    {
        Debug.Log("Event for movie end called");
        player.Stop();
        SceneManager.LoadScene("Login");
    }
}