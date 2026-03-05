using UnityEngine;
using System.Collections;

public class PortalTrigger : MonoBehaviour
{
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Player")
        {
            _animator.SetTrigger("Build");
        }
    }
}