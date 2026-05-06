using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    private Animator animator;
    public DoorManager manager; // ASIGNAR EN EL INSPECTOR
    private bool _isPressed = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !_isPressed)
        {
            _isPressed = true;
            animator.SetBool("isPressed", true);
            manager.PlateActivated(); // Avisamos al gestor
        }
    }
/*
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") && _isPressed)
        {
            _isPressed = false;
            animator.SetBool("isPressed", false);
            manager.PlateDeactivated(); // Avisamos que se liberó
        }
    }
*/
}