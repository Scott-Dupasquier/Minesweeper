using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Singleton object
    public static MenuManager menuManager;

    // Sprites for toggling music/sfx
    public Sprite musicOn;
    public Sprite musicOff;
    public Sprite sfxOn;
    public Sprite sfxOff;

    public Button music;
    public Button sfx;
    public Button home;

    public Animator menuAnimator;

    // Tracks if the menu is open or closed currently
    private bool open;
    private AudioManager audioManager;
    private SceneChanger sceneChanger;

    // Start is called before the first frame update
    void Start()
    {
        open = false;
        audioManager = FindObjectOfType<AudioManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();

        UpdateMusicImage();
        UpdateSFXImage();

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            home.interactable = false;
        }
    }

    public void ToggleMenu()
    {
        if (open)
        {
            menuAnimator.SetTrigger("CloseMenu");
            open = false;
        }
        else
        {
            menuAnimator.SetTrigger("OpenMenu");
            open = true;
        }
    }

    // Return home if not already there
    public void Home()
    {
        if (home.interactable)
        {
            sceneChanger.ChangeScene("MainMenu");
        }
    }

    // Toggle music on/off
    public void ToggleMusic()
    {
        audioManager.ToggleMusic();
        UpdateMusicImage();
    }

    // Toggle sfx on/off
    public void ToggleSFX()
    {
        audioManager.ToggleSFX();
        UpdateSFXImage();
    }

    private void UpdateMusicImage()
    {
        if (PlayerPrefs.GetString("music") == "on")
        {
            music.GetComponent<Image>().sprite = musicOn;
        }
        else
        {
            music.GetComponent<Image>().sprite = musicOff;
        }
    }

    private void UpdateSFXImage()
    {
        if (PlayerPrefs.GetString("sfx") == "on")
        {
            sfx.GetComponent<Image>().sprite = sfxOn;
        }
        else
        {
            sfx.GetComponent<Image>().sprite = sfxOff;
        }
    }
}
