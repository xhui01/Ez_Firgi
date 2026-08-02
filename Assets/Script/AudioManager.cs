using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Persistent singleton (survives scene loads via DontDestroyOnLoad) that owns the
/// background music AudioSource and the current mute state.
/// Put this on an empty GameObject in your VERY FIRST loaded scene (e.g. "Intro" or
/// "MainMenu") so it exists before any world/pause scene needs it. It will then live
/// for the entire app session - AddWorld, PausePage, etc. all just read/toggle it.
///
/// Set the music clip directly on the AudioSource component in the Inspector
/// (Loop = true), this script just plays whatever is already assigned there.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("Optional: boost volume past normal max on mobile")]
    [Tooltip("Only needed if the clip still sounds too quiet on phones after Normalize + AudioSource.Volume=1. " +
             "Route the AudioSource's Output to an Audio Mixer group, then this applies a positive dB boost - " +
             "AudioSource.volume alone caps at 0dB (1.0), but a Mixer can go louder than that.")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string mixerVolumeParam = "MusicVolume";
    [SerializeField] private float boostDb = 6f; // +6dB roughly doubles perceived loudness

    public bool IsMuted { get; private set; }

    private void Awake()
    {
        // Standard persistent singleton pattern: if one already exists
        // (e.g. re-entering MainMenu), destroy this duplicate instead.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (mixer != null && !string.IsNullOrEmpty(mixerVolumeParam))
        {
            mixer.SetFloat(mixerVolumeParam, boostDb);
        }

        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    /// <summary>
    /// Flips mute state and returns the NEW state, so callers (like the pause
    /// overlay's sound button) can immediately update their icon.
    /// </summary>
    public bool ToggleMute()
    {
        IsMuted = !IsMuted;
        if (musicSource != null) musicSource.mute = IsMuted;
        return IsMuted;
    }
}

//using UnityEngine;

///// <summary>
///// Persistent singleton (survives scene loads via DontDestroyOnLoad) that owns the
///// background music AudioSource and the current mute state.
///// Put this on an empty GameObject in your VERY FIRST loaded scene (e.g. "Intro" or
///// "MainMenu") so it exists before any world/pause scene needs it. It will then live
///// for the entire app session - AddWorld, PausePage, etc. all just read/toggle it.
/////
///// Set the music clip directly on the AudioSource component in the Inspector
///// (Loop = true), this script just plays whatever is already assigned there.
///// </summary>
//public class AudioManager : MonoBehaviour
//{
//    public static AudioManager Instance { get; private set; }

//    [Header("Background Music")]
//    [SerializeField] private AudioSource musicSource;

//    public bool IsMuted { get; private set; }

//    private void Awake()
//    {
//        // Standard persistent singleton pattern: if one already exists
//        // (e.g. re-entering MainMenu), destroy this duplicate instead.
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        if (musicSource != null && !musicSource.isPlaying)
//        {
//            musicSource.Play();
//        }
//    }

//    /// <summary>
//    /// Flips mute state and returns the NEW state, so callers (like the pause
//    /// overlay's sound button) can immediately update their icon.
//    /// </summary>
//    public bool ToggleMute()
//    {
//        IsMuted = !IsMuted;
//        if (musicSource != null) musicSource.mute = IsMuted;
//        return IsMuted;
//    }
//}
