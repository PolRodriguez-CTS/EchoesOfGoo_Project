using UnityEngine;

public class Potion : MonoBehaviour
{
    [SerializeField] private float healAmount = 1f; // Cuánto cura
    [SerializeField] private GameObject _coinVFXPrefab;
    [SerializeField] private Transform _vfxSpawnPoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Solo la usamos si el jugador no tiene la vida ya al máximo
                // (Opcional: puedes quitar esta condición si quieres que se consuma igual)
                //if (playerHealth.GetCurrentHealth() < 4)
                GameObject vfx = Instantiate(_coinVFXPrefab, _vfxSpawnPoint.position, _vfxSpawnPoint.rotation);
                
                SoundManager.PlaySound(SoundType.Potion);
                playerHealth.Heal(healAmount);
                
                    
                Destroy(gameObject);
            }
        }
    }
}
