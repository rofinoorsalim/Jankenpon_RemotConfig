using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneText(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void Replay()
    {
        var currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
