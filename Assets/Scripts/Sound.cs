using System;
using UnityEngine;
using UnityEngine.Audio;

// All credit to Brackeys https://www.youtube.com/watch?v=6OT43pvUyfY
[Serializable]
public class Sound
{
    // Basic variables for a sound clip to be played
    public AudioClip clip;

    public string clipName;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
