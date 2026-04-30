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
    }

    public void OpenAnimation()
    {
        animator.SetBool("isClosed", false);
    }
}
