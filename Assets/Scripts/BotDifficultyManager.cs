using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using System;

public class BotDifficultyManager : MonoBehaviour
{
    [SerializeField] Bot bot;
    [SerializeField] int selectedDifficulty;
    [SerializeField] BotStats[] botDifficulties;

    [Header("Remote Config Parameter:")]
    [SerializeField] bool enableRemotConfig = false;
    [SerializeField] string difficultyKey = "Difficulty";

    struct userAttributes{ };
    struct appAttributes { };
    IEnumerator Start()
    {
        //tunggu bot selesai set up
        yield return new WaitUntil(() => bot.isReady);

        //set stats default dari difficulty manager
        //sesuai selectedDifficulty dari inspector
        var newStats = botDifficulties[selectedDifficulty];
        bot.SetStats(newStats,true);

        //Ambil difficulty dari remote config kalau enabled
        if(enableRemotConfig == false)
        {
            yield break;
        }
        //tunggu sampai unity service siap
        yield return new WaitUntil(
            () =>
                UnityServices.State == ServicesInitializationState.Initialized
                &&
                AuthenticationService.Instance.IsSignedIn
            );
        //daftar dulu untuk eventfetch completed
        RemoteConfigService.Instance.FetchCompleted += OnRemoteConfigFetched;
        //fetch disini . cukup sekali diawal permainan
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(),new appAttributes());
    }

    private void OnDestroy()
    {
        //jangan lupa untuk unregister event untuk menghindari memory leak
        RemoteConfigService.Instance.FetchCompleted -= OnRemoteConfigFetched;
    }

    //setiap kali data baru didapatkan (melalui fetch) fungsi akan dipanggil
    private void OnRemoteConfigFetched(ConfigResponse response)
    {
        if (RemoteConfigService.Instance.appConfig.HasKey(difficultyKey) == false)
        {
            Debug.LogWarning($"Difficulty Key:{difficultyKey} not found on remote config server");
            //return;
        }
        switch (response.requestOrigin)
        {
            case ConfigOrigin.Default:
            case ConfigOrigin.Cached:
                break;
            case ConfigOrigin.Remote:
                selectedDifficulty = RemoteConfigService.Instance.appConfig.GetInt(difficultyKey);
                selectedDifficulty = Mathf.Clamp(selectedDifficulty, 0, botDifficulties.Length - 1);
                var newStats = botDifficulties[selectedDifficulty];
                bot.SetStats(newStats,true);
                break;
        }
    }
}
