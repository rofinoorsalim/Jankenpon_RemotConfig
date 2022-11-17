using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using DG.Tweening;

public class ShooterPlayer : MonoBehaviourPun
{
    [SerializeField] float speed  = 5;
    [SerializeField] int health = 10;

    [SerializeField] TMP_Text playerName;

    private void Start()
    {
        playerName.text = photonView.Owner.NickName + $"({health})";
    }

    private void Update()
    {
        if (photonView.IsMine ==false)
        {
            return;
        }
        Vector2 moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        transform.Translate(moveDir*Time.deltaTime* speed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            photonView.RPC("takeDamage", RpcTarget.All,1);
        }
    }

    [PunRPC]
    public void takeDamage(int amount)
    {
        health -= amount;
        playerName.text = photonView.Owner.NickName + $"({health})";
        GetComponent<SpriteRenderer>().DOColor(Color.red, 0.2f).SetLoops(1, LoopType.Yoyo).From();
    }
}
