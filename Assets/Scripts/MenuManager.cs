using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Sprites for toggling music/sfx
    public Sprite musicOn;
    public Sprite musicOff;
    public Sprite sfxOn;
    public Sprite sfxOff;

    // Buttons in the menu
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
        // Get the AudioManager and SceneChanger as they're used heavily for the menu
        open = false;
        audioManager = FindObjectOfType<AudioManager>();
        sceneChanger = FindObjectOfType<SceneChanger>();

        // Set images
        UpdateMusicImage();
        UpdateSFXImage();

        // Home button is inactive if they're already on the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            home.interactable = false;
        }
    }

    // Opens or closes the menu
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

    // Sets the music button image based on whether the music is on or off
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

    // Sets the sfx button image based on whether the music is on or off
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
