using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        string inputSeedField = GameObject.Find("SeedField").GetComponent<InputField>().text;
        int seedNumber;

        if (inputSeedField != null && inputSeedField != "" && int.TryParse(inputSeedField, out seedNumber))
        {
            GameObject.Find("SeedField").SetActive(false);
            GameObject.Find("SeedWarning").GetComponent<Text>().enabled = false;
            TerrainGenerator.Seed = seedNumber;
            LoadingScreen loadingScreen = GameObject.Find("Loading").GetComponent<LoadingScreen>();
            loadingScreen.LoadGame();
        }

        else 
        {
            GameObject.Find("SeedWarning").GetComponent<Text>().enabled = true;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }

}
