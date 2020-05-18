using System;
using UnityEngine;
using UnityEngine.Audio;

// All credit to Brackeys https://www.youtube.com/watch?v=6OT43pvUyfY
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager audioManager;

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
    }

    void Start()
    {
        // Start the main theme music
        PlaySound("theme");
    }

    public void PlaySound(string clipName)
    {
        Sound toPlay = Array.Find(sounds, sound => sound.clipName == clipName);
        if (toPlay != null)
        {
            toPlay.source.Play();
        }
    }
}
