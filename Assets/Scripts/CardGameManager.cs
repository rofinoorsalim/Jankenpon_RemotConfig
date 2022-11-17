using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CardGameManager : MonoBehaviour, IOnEventCallback
{
    public GameObject netPlayerPrefabs;
    public CardPlayer P1;
    public CardPlayer P2;

    public SoundManager soundManager;

    public PlayerStats defaultPlayerStat = new PlayerStats
    {
        MaxHealth = 100,
        RestoreValue = 5,
        DamageValue = 10
    };

    public GameState State, NextState = GameState.NetPlayerInitialization;

    public GameObject gameOverPanel;

    public TMP_Text winnerText;
    public TMP_Text pingText;

    private CardPlayer damagedPlayer;
    private CardPlayer winner;

    public bool online;

    //public List<int> syncReadyPlayers = new List<int>(2);
    HashSet<int> syncReadyPlayers = new HashSet<int>();

    public enum GameState
    {
        SynState,
        NetPlayerInitialization,
        ChooseAttack,
        Attacks,
        Damages,
        Draw,
        GameOver,
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        if (online)
        {
            PhotonNetwork.Instantiate(netPlayerPrefabs.name, Vector3.zero, Quaternion.identity);
            StartCoroutine(PinCoroutine());

            State = GameState.NetPlayerInitialization;
            NextState = GameState.NetPlayerInitialization;
        }
        else
        {
            State = GameState.ChooseAttack;
        }

        P1.SetStats(defaultPlayerStat,true);
        P2.SetStats(defaultPlayerStat,true);
        P1.isReady = true;
        P2.isReady = true;
    }

    private void Update()
    {
        // ChooseAttack
        switch (State)
        {
            case GameState.SynState:
                if (syncReadyPlayers.Count == 2)
                {
                    syncReadyPlayers.Clear();
                    State = NextState;
                }
                break;
            case GameState.NetPlayerInitialization:
                if (CardNetPlayer.NetPlayers.Count == 2)
                {
                    foreach (var netPlayer in CardNetPlayer.NetPlayers)
                    {
                        if (netPlayer.photonView.IsMine)
                        {
                            netPlayer.Set(P1);
                        }
                        else
                        {
                            netPlayer.Set(P2);
                        }
                    }
                    ChangeState(GameState.ChooseAttack);
                }
                break;

            case GameState.ChooseAttack:
                if (P1.AttackValue != null && P2.AttackValue != null)
                {
                    P1.AnimateAttack();
                    P2.AnimateAttack();
                    P1.IsClickable(false);
                    P2.IsClickable(false);
                    ChangeState(GameState.Attacks);
                    //SoundManager.instance.ClickStart();
                }
                break;

            case GameState.Attacks:
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    damagedPlayer = GetDamagedPlayer();

                    if (damagedPlayer != null)
                    {
                        damagedPlayer.AnimateDamage();
                        ChangeState(GameState.Damages);
                        //SoundManager.instance.HitStart();
                    }
                    else
                    {
                        P1.AnimateDraw();
                        P2.AnimateDraw();
                        ChangeState(GameState.Draw);
                    }
                }
                break;

            case GameState.Damages:
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    // Hitung darah
                    if (damagedPlayer == P1)
                    {
                        P1.ChangeHealth(-P2.stats.DamageValue);
                        P2.ChangeHealth(P2.stats.RestoreValue);
                    }
                    else
                    {
                        P1.ChangeHealth(P1.stats.RestoreValue);
                        P2.ChangeHealth(-P1.stats.DamageValue);
                    }

                    var winner = GetWinner();

                    if (winner == null)
                    {
                        ResetPlayers();
                        P1.IsClickable(true);
                        P2.IsClickable(true);
                        ChangeState(GameState.ChooseAttack);
                    }
                    else
                    {
                        soundManager.WinSoundStart();
                        gameOverPanel.SetActive(true);
                        winnerText.text = winner == P1 ? $"{P1.Nickname.text} WIN!" : $"{P2.Nickname.text} WIN!";
                        //SoundManager.instance.WinSoundStart();
                        ResetPlayers();
                        ChangeState(GameState.GameOver);
                    }
                }
                break;

            case GameState.Draw:
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    ResetPlayers();
                    P1.IsClickable(true);
                    P2.IsClickable(true);
                    ChangeState(GameState.ChooseAttack);
                }
                break;
        }

    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }


    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private const byte playerChangeState = 1;

    private void ChangeState(GameState newState)
    {
        if (online == false)
        {
            State = newState;
            return;
        }

        if (this.NextState == newState)
            return;

        var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        var raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(playerChangeState, actorNum, raiseEventOptions, SendOptions.SendReliable);
        this.State = GameState.SynState;
        this.NextState = newState;
    }

    public void OnEvent(EventData photonEvent)
    {
        //if (photonEvent.Code == playerChangeState)
        //{
        //var actorNum = (int)photonEvent.CustomData;

        //if (syncReadyPlayers.Contains(actorNum) == false)
        //{
        //syncReadyPlayers.Add(actorNum);
        //}
        //}
        switch (photonEvent.Code)
        {
            case playerChangeState:
                var actorNum = (int)photonEvent.CustomData;
                syncReadyPlayers.Add(actorNum);
                break;
            default:
                break;
        }
    }

    IEnumerator PinCoroutine()
    {
        var wait = new WaitForSeconds(1);
        while (true)
        {
            pingText.text = "ping: " + PhotonNetwork.GetPing() + "ms";
            yield return wait;

        }
    }

    private void ResetPlayers()
    {
        damagedPlayer = null;
        P1.Reset();
        P2.Reset();
    }
    private CardPlayer GetDamagedPlayer()
    {
        Attack? PlayerAtk1 = P1.AttackValue;
        Attack? PlayerAtk2 = P2.AttackValue;

        if (PlayerAtk1 == Attack.Rock && PlayerAtk2 == Attack.Paper)
            return P1;
        else if (PlayerAtk1 == Attack.Rock && PlayerAtk2 == Attack.Scissor)
            return P2;
        else if (PlayerAtk1 == Attack.Paper && PlayerAtk2 == Attack.Rock)
            return P2;
        else if (PlayerAtk1 == Attack.Paper && PlayerAtk2 == Attack.Scissor)
            return P1;
        else if (PlayerAtk1 == Attack.Scissor && PlayerAtk2 == Attack.Rock)
            return P1;
        else if (PlayerAtk1 == Attack.Scissor && PlayerAtk2 == Attack.Paper)
            return P2;

        return null;
    }

    private CardPlayer GetWinner()
    {
        if (P1.Health == 0)
        {
            return P2;
        }
        else if (P2.Health == 0)
        {
            return P1;
        }
        else
        {
            return null;
        }
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
