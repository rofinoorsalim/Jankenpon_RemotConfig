using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;

    public void Set(Photon.Realtime.Player player)
    {
        playerName.text = player.NickName;
        if (player == PhotonNetwork.MasterClient)
        {
            playerName.text += " (Master)";
        }
    }
}
