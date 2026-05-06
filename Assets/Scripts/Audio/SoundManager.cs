using UnityEngine;
public enum SoundType
{
    Attack1,
    Attack2,
    Attack3,
    Heavy1,
    Heavy2,
    Jump,
    Boost,
    ArgylDamaged,
    ArgylDeath,
    GolemHit,
    GolemExplode,
    GolemStun,
    Coin,
    Potion,
    OpenDoor,
    CloseDoor,
    PortalDash,
    DefensorScream
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClips;
    private static SoundManager Instance;
    private AudioSource audioSource;
    [SerializeField] private AudioSource loopSource;

    private void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();

        if (loopSource != null)
        {
            loopSource.loop = true;
            loopSource.playOnAwake = false;
        }
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        Instance.audioSource.PlayOneShot(Instance.audioClips[(int)sound], volume);
    }

    public static void PlayLoop(SoundType sound, float volume = 1)
    {
        if (Instance.loopSource.isPlaying) return; // Evita que se reinicie si ya suena
        
        Instance.loopSource.clip = Instance.audioClips[(int)sound];
        Instance.loopSource.volume = volume;
        Instance.loopSource.Play();
    }

    public static void StopLoop()
    {
        Instance.loopSource.Stop();
    }
}
