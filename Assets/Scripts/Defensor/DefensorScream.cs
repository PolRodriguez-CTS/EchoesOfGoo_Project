using UnityEngine;

public class DefensorScream : MonoBehaviour
{
    public void Scream()
    {
        SoundManager.PlaySound(SoundType.DefensorScream);
    }
}
