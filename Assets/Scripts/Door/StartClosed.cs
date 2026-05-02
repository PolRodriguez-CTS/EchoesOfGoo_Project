using System.Collections;
using UnityEngine;

public class StartClosed : MonoBehaviour
{
    private Animator animator;

    IEnumerator Start()
    {
        yield return null;
        animator = GetComponent<Animator>();
        animator.SetBool("isClosed", true);
    }
}