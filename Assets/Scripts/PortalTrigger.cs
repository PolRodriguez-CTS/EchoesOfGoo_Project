using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    private Animator _animator;
    private bool _yaSeActivo = false; // El cerrojo de seguridad

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Comprobamos que sea el Player
        // 2. Comprobamos que no esté ya abierto
        if (other.CompareTag("Player") && !_yaSeActivo)
        {
            _yaSeActivo = true; // Cerramos el paso para que no se repita
            
            // Usamos SetTrigger como tenías, pero ahora solo pasará UNA vez
            _animator.SetTrigger("Build"); 

            Debug.Log("¡Portal activado!");
        }
    }
}