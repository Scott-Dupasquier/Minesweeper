using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger sceneChanger;

    // Singleton object, on awake make sure we don't repeat
    void Awake()
    {
        if (!sceneChanger)
        {
            sceneChanger = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Changes to main menu
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Changes to game screen
    public void GameScreen()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Exit the application
    public void Quit()
    {
        Application.Quit();
    }
}
