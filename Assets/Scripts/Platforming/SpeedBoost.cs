using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _boostForce = 15f; // Fuerza adicional
    [SerializeField] private float _boostDuration = 0.5f; // Cuánto dura el impulso

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo que entró tiene nuestro script de PlayerController
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            // Calculamos la dirección (en este caso, hacia donde mira el objeto turbo)
            Vector3 boostDirection = transform.forward;
            
            // Llamamos a un método público en el player
            player.ApplyExternalImpulse(boostDirection * _boostForce, _boostDuration);
        }
    }

    // Dibujamos una flecha en el editor para saber hacia dónde apunta el turbo
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}