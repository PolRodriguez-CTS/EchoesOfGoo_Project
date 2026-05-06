using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void CloseAnimation()
    {
        animator.SetBool("isClosed", true);
        SoundManager.PlaySound(SoundType.CloseDoor, 0.6f);
    }

    public void OpenAnimation()
    {
        animator.SetBool("isClosed", false);
        SoundManager.PlaySound(SoundType.OpenDoor, 0.6f);
    }
}