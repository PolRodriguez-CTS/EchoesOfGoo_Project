using UnityEngine;

public class WallSpikes : MonoBehaviour
{
    [Header("Ajustes de Empuje")]
    [SerializeField] private float _pushForce = 25f; // Fuerza hacia afuera
    //[SerializeField] private float _verticalLift = 5f; // Fuerza hacia arriba

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null)
            {
                Vector3 pushDirection = - transform.up;

                Vector3 finalForce = (pushDirection * _pushForce);

                player.ApplyKnockback(finalForce);

                player.GetComponent<PlayerHealth>().Damaged(1);
            }
        }
    }
}
