using UnityEngine;
public enum SoundType
{
    Player,
    Golems,
    UI
}

public enum UISoundType
{
    Select,
    Interact,
    Close
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private AudioClip[] uiAudioClips;
    private static SoundManager Instance;
    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        Instance.audioSource.PlayOneShot(Instance.audioClips[(int)sound], volume);
    }

    public static void PlayUISound(UISoundType uiSound, float volume = 1)
    {
        Instance.audioSource.PlayOneShot(Instance.uiAudioClips[(int)uiSound], volume);
    }
}
