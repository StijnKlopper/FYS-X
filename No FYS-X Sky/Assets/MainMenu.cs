using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public void PlayGame ()
    {
        LoadingScreen loadingScreen = GameObject.Find("Loading").GetComponent<LoadingScreen>();
        loadingScreen.LoadGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }

}
