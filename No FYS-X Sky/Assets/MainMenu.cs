using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    Image loadingImage;

    private void Start()
    {
        loadingImage = GameObject.Find("Loading").GetComponent<Image>();
        loadingImage.enabled = false;
    }

    public void PlayGame ()
    {
        loadingImage.enabled = true;

        // In Unity -> File -> Build Settings are the scene ID's
        // MainMenuScene is 0
        // GameScene is 1
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }

}
