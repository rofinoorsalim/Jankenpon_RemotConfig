using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class ConnectManger : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_Text feedbackText;
    public void ClickConnect()
    {
        feedbackText.text = "";
        if (usernameInput.text.Length < 3)
        {
            feedbackText.text = "Username min 3 character";
            return;
        }

        //simpan username
        PhotonNetwork.NickName = usernameInput.text;
        PhotonNetwork.AutomaticallySyncScene = true;
        //simpan ke server
        PhotonNetwork.ConnectUsingSettings();
        feedbackText.text = "Connecting...";
    }

    //dijalankan ketika sudah connect
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        feedbackText.text = "Connecter to Master";
        StartCoroutine(LoadLevelAfterConnectAndReady());
    }

    IEnumerator LoadLevelAfterConnectAndReady()
    {
        while(PhotonNetwork.IsConnectedAndReady == false)
        {
            yield return null;
        }
        SceneManager.LoadScene("Lobby");
    }
}
