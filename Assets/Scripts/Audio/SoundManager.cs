using UnityEngine;
public enum SoundType
{
    Attack1,
    Attack2,
    Attack3,
    Heavy1,
    Heavy2,
    Jump,
    Boost
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClips;
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
}
