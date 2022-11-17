using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField newRoomInputField;
    [SerializeField] TMP_Text feedbackText;

    [SerializeField] Button StartGameButton;

    [SerializeField] GameObject roomPanel;
    [SerializeField] TMP_Text roomNameText;

    [SerializeField] GameObject RoomListObject;
    [SerializeField] GameObject PlayerListObject;

    [SerializeField] RoomItem roomItemPrefabs;
    [SerializeField] PlayerItem playerItemPrefabs;

    List<RoomItem> roomItemlist = new List<RoomItem>();
    List<PlayerItem> playerItemlist = new List<PlayerItem>();

    Dictionary<string, RoomInfo> roomInfoCache = new Dictionary<string, RoomInfo>();
    private void Start()
    {
        feedbackText.text = "Joining Lobby";
        PhotonNetwork.JoinLobby();
        roomPanel.SetActive(false);
    }
    public void ClickCreateRoom()
    {
        feedbackText.text = "";
        if(newRoomInputField.text.Length < 3)
        {
            feedbackText.text = "Room Name min 3 Character";
            return;
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;
        PhotonNetwork.CreateRoom(newRoomInputField.text,roomOptions);
    }

    public void ClickStartGame(string levelname)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(levelname);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room: " + PhotonNetwork.CurrentRoom.Name);
        feedbackText.text = "Created room: " + PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        feedbackText.text = "Joined room: " + PhotonNetwork.CurrentRoom.Name;
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        roomPanel.SetActive(true);

        //update player list
        UpdatePlayerList();

        //atur start game button
        SetStartGameButton();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        //update player list
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //update player list
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        //atur start game button
        SetStartGameButton();
    }

    private void SetStartGameButton()
    {
        //hanya tampil di master
        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        //bisa diclick ketika pemanin lebih dari 1
        StartGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 2;
    }

    public void UpdatePlayerList()
    {
        //destroy semua player yang sudah ada
        foreach (var item in playerItemlist)
        {
            Destroy(item.gameObject);
        }

        playerItemlist.Clear();

        //bikin ulang player list
        //foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)(Alternative)
        foreach(var (id,player) in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefabs, PlayerListObject.transform);
            newPlayerItem.Set(player);
            playerItemlist.Add(newPlayerItem);

            if(player == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.transform.SetAsFirstSibling();
            }
        }

        //start game hanya bisa diklik ketika jumlah pemain tertentu
        SetStartGameButton();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(returnCode+","+ message);
        feedbackText.text = returnCode.ToString() + ": "+message;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var roomInfo in roomList)
        {
            roomInfoCache[roomInfo.Name] = roomInfo;
        }

        foreach (var item in roomItemlist)
        {
            Destroy(item.gameObject);
        }
        this.roomItemlist.Clear();
        var roomInfoList = new List<RoomInfo>(roomInfoCache.Count);
        //sort yang open di add duluan
        foreach(var roomInfo in roomInfoCache.Values)
        {
            if (roomInfo.IsOpen)
            {
                roomInfoList.Add(roomInfo);
            }
        }
        // kemudian yang close
        foreach (var roomInfo in roomInfoCache.Values)
        {
            if (roomInfo.IsOpen == false)
            {
                roomInfoList.Add(roomInfo);
            }
        }
        foreach (var roomInfo in roomInfoList)
        {
            if (roomInfo.IsVisible == false ||roomInfo.MaxPlayers == 0 )
            {
                continue;
            }
            RoomItem newRoomItem = Instantiate(roomItemPrefabs, RoomListObject.transform);
            newRoomItem.Set(this, roomInfo);
            roomItemlist.Add(newRoomItem);
        }
    }
}
