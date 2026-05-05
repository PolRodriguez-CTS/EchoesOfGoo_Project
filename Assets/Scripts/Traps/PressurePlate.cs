using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    //Variables
    private Animator animator;
    private bool _isPlayerOver = false;


    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            animator.SetBool("isPressed", true);
            //Llamar funcion desde evento de animación
        }
    }

    void Activate()
    {
        //Funciones situacionales dependiendo de que queremos que haga la placa de presión
    }
}
