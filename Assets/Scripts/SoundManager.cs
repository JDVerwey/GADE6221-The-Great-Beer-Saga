using UnityEngine;

using UnityEngine;

using UnityEngine;

//Enum of sounds
public enum SoundType
{
    Run,
    Collide,
    Music,
    Shield,
    Meat,
    Berry
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList; //Array of sound clips
    private static SoundManager instance; //Static reference to SoundManager
    private AudioSource sfxAudioSource; //AudioSource for sound effects
    private AudioSource musicAudioSource; //AudioSource for background music

    private void Awake()
    {
        if (instance == null) //Singleton setup
        {
            instance = this;
            DontDestroyOnLoad(gameObject); //Persist across scenes
        }
        else
        {
            Destroy(gameObject); //Destroy duplicate
            return;
        }

        sfxAudioSource = GetComponent<AudioSource>(); //Initialize AudioSource for sfx
        musicAudioSource = gameObject.AddComponent<AudioSource>(); //Initialize AudioSource for music
    }

    private void Start()
    {
        PlayBackgroundMusic(SoundType.Music); //Play background music
    }

    public static void PlaySound(SoundType sound, float volume = 1) //Play sound effects
    {
        if (sound != SoundType.Music) //Exclude music from PlayOneShot
        {
            instance.sfxAudioSource.PlayOneShot(instance.soundList[(int)sound], volume);
        }
        else
        {
            Debug.LogWarning("Use PlayBackgroundMusic for Music instead of PlaySound."); //Warn if music is called incorrectly
        }
    }

    public static void PlayBackgroundMusic(SoundType sound) //Play and loop background music
    {
        if (sound == SoundType.Music && instance.soundList[(int)sound] != null) //Check for valid music clip
        {
            instance.musicAudioSource.clip = instance.soundList[(int)sound]; //Configure music
            instance.musicAudioSource.loop = true; //Enable looping
            instance.musicAudioSource.volume = 0.5f; //Set volume to half
            instance.musicAudioSource.Play(); //Play music
        }
        else
        {
            Debug.LogWarning("Music clip is not assigned or invalid SoundType for background music."); //Warn if music clip is missing
        }
    }

    public static void StopBackgroundMusic() //Stop background music
    {
        if (instance.musicAudioSource.isPlaying) //Check if music is playing
        {
            instance.musicAudioSource.Stop(); //Stop music
        }
    }
}
