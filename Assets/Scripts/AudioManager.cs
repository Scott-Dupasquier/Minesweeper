using System;
using UnityEngine;
using UnityEngine.Audio;

// Credit to Brackeys https://www.youtube.com/watch?v=6OT43pvUyfY
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager audioManager;

    // Player preferences on whether they want sound or not
    private string playMusic;
    private string playSFX;

    void Awake()
    {
        // Singleton object, don't repeat
        if (!audioManager)
        {
            audioManager = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Going to be used through other scenes
        DontDestroyOnLoad(gameObject);
        
        // Adjust all values to what's specified
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        // Make default PlayerPrefs if they don't exist
        if (!PlayerPrefs.HasKey("music"))
        {
            PlayerPrefs.SetString("music", "on");
            PlayerPrefs.SetString("sfx", "on");
        }
    }

    void Start()
    {
        // Get basic preferences
        playMusic = PlayerPrefs.GetString("music");
        playSFX = PlayerPrefs.GetString("sfx");

        // Start the main theme music
        if (playMusic == "on")
        {
            PlaySound("theme");
        }
    }

    // Toggle the music on and off
    public void ToggleMusic()
    {
        if (playMusic == "on")
        {
            playMusic = "off";
            PauseSound("theme");
        }
        else
        {
            playMusic = "on";
            PlaySound("theme");
        }
        PlayerPrefs.SetString("music", playMusic);
    }

    // Toggle sfx on and off
    public void ToggleSFX()
    {
        if (playSFX == "on")
        {
            playSFX = "off";
        }
        else
        {
            playSFX = "on";
        }
        PlayerPrefs.SetString("sfx", playSFX);
    }


    // Play a sound
    public void PlaySound(string clipName)
    {
        // Get specified sound from the array
        Sound toPlay = Array.Find(sounds, sound => sound.clipName == clipName);
        
        // Two conditions to play the sound, one must be met for it to play
        // Either the player wants sound effects and we aren't wanting to play the theme
        // or the player wants the music and the clip name is theme
        var cond1 = playSFX == "on" && clipName != "theme";
        var cond2 = playMusic == "on" && clipName == "theme";

        // Check one of the conditions is met and the clip exists
        if (toPlay != null && (cond1 || cond2))
        {
            toPlay.source.Play();
        }
    }

    // Pause a sound
    private void PauseSound(string clipName)
    {
        // Locate sound clip from the array
        Sound toPlay = Array.Find(sounds, sound => sound.clipName == clipName);
        if (toPlay != null)
        {
            // Pause sound clip
            toPlay.source.Pause();
        }
    }
}
