using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private Image loadingImage;

    private TextMeshProUGUI loadingIndicator;

    private float nextActionTime = 0.0f;
    private float period = 1.0f;

    private int loadingDots = 3;

    void Start()
    {
        loadingImage = GameObject.Find("Loading").GetComponent<Image>();
        loadingIndicator = GameObject.Find("LoadingIndicator").GetComponent<TextMeshProUGUI>();

        loadingImage.enabled = false;
        loadingImage.sprite = GetRandomBackground();
    }

    void Update()
    {
        // The changing amount of dots is not visible because the game loads too fast
        if (loadingImage.enabled && Time.time > nextActionTime )
        {
            nextActionTime += period;

            if (loadingDots > 3) loadingDots = 0;

            string text = "Loading Game" + string.Concat(Enumerable.Repeat(".", loadingDots)); ;

            loadingDots += 1;

            loadingIndicator.text = text;
        }
    }

    public void LoadGame()
    {
        // Make the loading Image visible
        loadingImage.color = new Color(1f, 1f, 1f, 1f);
        loadingImage.enabled = true;

        // In Unity -> File -> Build Settings the scene IDs can be found
        // MainMenuScene is 0
        // GameScene is 1
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private Sprite GetRandomBackground()
    {
        // Get a random background from the folder Assets/Resources/Images/LoadingScreens
        Sprite sprite;
        System.Random random = new System.Random();

        Sprite[] backgrounds = Resources.LoadAll<Sprite>("Images/LoadingScreens");
    
        sprite = backgrounds[random.Next(0, backgrounds.Length)];
        return sprite;
    }
}
