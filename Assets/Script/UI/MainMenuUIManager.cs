using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
public class MainMenuUIManager : MonoBehaviour {


    public static MainMenuUIManager Instance { get; private set; }

   
    [SerializeField] private GameObject gamePlayPreference;
    [SerializeField] private GameObject ConnectPVP;
  

	private int playerCount = 2;

  
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
            Instance = this;

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (gamePlayPreference.activeSelf) {
				gamePlayPreference.SetActive (false);
			} else {
				//quitDialog.ShowDialog ("Are you sure want to quit?", () => Application.Quit (), null);
			}
		}
	}

	public void OnVSComputer ()
	{
		gamePlayPreference.SetActive (true);
	}
    public void OnPVP()
    {
        ConnectPVP.SetActive(true);
        OnPlay();
    }

    public void OnPlay ()
	{


        Debug.Log("Game Play");
        SceneManager.LoadScene("GamePlay");
        PhotonNetwork.LoadLevel("GamePlay");
        //  SceneManager.LoadScene ("GamePlay");
    }

}
