using UnityEngine;

public class StartClosed : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isClosed", true);
    }
}