using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        LoadingScreen loadingScreen = GameObject.Find("Loading").GetComponent<LoadingScreen>();
        loadingScreen.LoadGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        //EditorApplication.isPlaying = false;
    }

}
