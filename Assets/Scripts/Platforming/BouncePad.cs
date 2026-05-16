using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _bounceHeight = 8f; // Altura aproximada del salto
    [SerializeField] private bool _resetDoubleJump = true; // ¿Permite volver a usar el doble salto después de rebotar?

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo que entró tiene nuestro script de PlayerController
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            // Llamamos a un nuevo método público en el player
            player.ApplyBounce(_bounceHeight, _resetDoubleJump);
            
            // Opcional: Aquí podrías reproducir un sonido o una animación de la cama elástica
            SoundManager.PlaySound(SoundType.Mushroom, 0.7f);
        }
    }

    // Dibujamos un visual en el editor para identificar la cama elástica
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        // Dibujamos un muelle esquemático
        Vector3 pos = transform.position;
        for (int i = 0; i < 5; i++)
        {
            Gizmos.DrawWireSphere(pos + Vector3.up * i * 0.2f, 0.5f * (1f - i*0.1f));
        }
    }
}