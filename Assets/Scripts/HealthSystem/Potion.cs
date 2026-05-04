using UnityEngine;

public class Potion : MonoBehaviour
{
    [SerializeField] private float healAmount = 1f; // Cuánto cura

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Solo la usamos si el jugador no tiene la vida ya al máximo
                // (Opcional: puedes quitar esta condición si quieres que se consuma igual)
                if (playerHealth.GetCurrentHealth() < 4) 
                {
                    playerHealth.Heal(healAmount);
                    
                    // Destruimos la poción para que no se pueda usar infinitamente
                    Destroy(gameObject);
                }
            }
        }
    }
}
