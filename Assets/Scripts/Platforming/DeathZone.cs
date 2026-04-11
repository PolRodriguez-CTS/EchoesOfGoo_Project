using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 1. Ver si algo está entrando al trigger
        Debug.Log("Algo entró en la zona: " + other.name);

        if(other.CompareTag("Player"))
        {
            Debug.Log("¡Es el Player!");

            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if(health != null)
            {
                Debug.Log("Script PlayerHealth encontrado. Aplicando daño...");
                health.Damaged(1); // Usamos un número alto para asegurar la muerte
                health.ReturnToCheckpoint();
            }
            else
            {
                Debug.LogError("El objeto tiene el tag Player pero NO tiene el script PlayerHealth colgado.");
            }
        }
    }
}