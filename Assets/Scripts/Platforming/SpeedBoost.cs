using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    private Animator animator;

    [Header("Settings")]
    [SerializeField] private float _boostForce = 15f; // Fuerza adicional

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo que entró tiene nuestro script de PlayerController
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            // Calculamos la dirección (en este caso, hacia donde mira el objeto turbo)
            Vector3 boostDirection = transform.forward;
            
            animator.SetTrigger("airDash");
            SoundManager.PlaySound(SoundType.PortalDash);
            
            // Llamamos a un método público en el player
            player.ApplyExternalImpulse(boostDirection * _boostForce);
        }
    }

    // Dibujamos una flecha en el editor para saber hacia dónde apunta el turbo
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}