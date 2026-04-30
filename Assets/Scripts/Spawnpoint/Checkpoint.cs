using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos si es el jugador
        if (other.CompareTag("Player"))
        {
            // 2. Buscamos el script de vida en el jugador
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 3. Le pasamos este mismo objeto como nuevo spawnPoint
                // Usamos transform porque el script PlayerHealth pide un Transform
                playerHealth.UpdateSpawnPoint(this.transform);
                
                Debug.Log("¡Checkpoint alcanzado!: " + gameObject.name);
            }
        }
    }
}