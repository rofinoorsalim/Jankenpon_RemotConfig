using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class CardPlayer : MonoBehaviour
{
    public Transform atkPoskRef;
    public Card choosenCard;
    public HealthBar healthBar;

    [SerializeField] private TMP_Text nicknameText;
    public TMP_Text healthText;

    public float Health;

    public PlayerStats stats = new PlayerStats {
        MaxHealth = 100,
        RestoreValue = 5,
        DamageValue = 10
    };

    private Tweener animationTween;
    public TMP_Text Nickname { get => nicknameText;}

    public bool isReady = false;

    //public AudioSource audioSource;
    //public AudioClip damageClip;
    public SoundManager soundManager;
    public void Start()
    {
        Health = stats.MaxHealth;
    }

    public void SetStats(PlayerStats newStats, bool restoreFullHealth = false)
    {
        this.stats = newStats;
        if (restoreFullHealth)
        {
            Health = stats.MaxHealth;
        }
        UpdateHealthBar();
    }

    public Attack? AttackValue
    {
        get => choosenCard == null ? null : choosenCard.AttackValue;
    }

    public void Reset()
    {
        if (choosenCard != null)
        {
            choosenCard.Reset();
        }

        choosenCard = null;
    }

    public void SetChoosenCard(Card newCard)
    {

        if (choosenCard != null)
        {
            choosenCard.transform.DOKill();
            choosenCard.Reset();
        }

        choosenCard = newCard;
        choosenCard.transform.DOScale(choosenCard.transform.localScale * 1.2f, 0.2f);
    }

    public void ChangeHealth(float amount)
    {
        Health += amount;
        Health = Math.Clamp(Health, 0, stats.MaxHealth);
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        // healthbar
        healthBar.UpdateBar(Health / stats.MaxHealth);

        //text
        healthText.text = Health + " / " + stats.MaxHealth;
    }

    public void AnimateAttack()
    {
        animationTween = choosenCard.transform.DOMove(atkPoskRef.position, 1);
    }

    public void AnimateDraw()
    {
        soundManager.DrawStart();
        animationTween = choosenCard.transform
            .DOMove(choosenCard.OriginalPosition, 0.8f)
            .SetEase(Ease.InBack)
            .SetDelay(0.2f);
    }

    public void AnimateDamage()
    {
        //audioSource.PlayOneShot(damageClip);
        soundManager.HitStart();
        var image = choosenCard.GetComponent<Image>();
        animationTween = image
            .DOColor(Color.red, 0.1f)
            .SetLoops(3, LoopType.Yoyo)
            .SetDelay(0.2f);
    }

    public bool IsAnimating()
    {
        return animationTween.IsActive();
    }

    public void IsClickable(bool value)
    {
        Card[] cards = GetComponentsInChildren<Card>();
        foreach (var card in cards)
        {
            card.SetClickable(value);
        }
    }
}
