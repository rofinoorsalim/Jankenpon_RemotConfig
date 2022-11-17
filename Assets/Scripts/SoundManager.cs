using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioClip WinSound;
    public AudioClip Hit;
    public AudioClip Draw;

    public AudioSource BackGround;

    public AudioSource audio;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(WinSound);
        }
        else
        {
            instance = this;
        }
        audio = GetComponent<AudioSource>();
    }

    public void WinSoundStart()
    {
        BackGround.Stop();
        audio.PlayOneShot(WinSound);
    }

    public void HitStart()
    {
        audio.PlayOneShot(Hit);
    }

    public void DrawStart()
    {
        audio.PlayOneShot(Draw);
    }
}
