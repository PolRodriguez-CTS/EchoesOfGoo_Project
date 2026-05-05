using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float _pushForce = 20f;
    [SerializeField] private float _verticalLift = 5f;

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            PlayerHealth _playerHealthScript = collision.gameObject.GetComponent<PlayerHealth>();
            PlayerController _playerControllerScript = collision.gameObject.GetComponent<PlayerController>();
            
            if(_playerControllerScript != null && _playerHealthScript != null)
            {
                // 1. Calculamos la dirección del empuje
                // OPCIÓN A: Empuje desde el centro de la trampa hacia afuera
                Vector3 pushDir = (collision.transform.position - transform.position);

                pushDir.y = 0; // Limpiamos el eje Y para controlarlo nosotros
                pushDir = pushDir.normalized;

                // 2. Combinamos fuerza horizontal y vertical
                Vector3 finalForce = (pushDir * _pushForce) + (Vector3.up * _verticalLift);

                // 3. Aplicamos al Player

                _playerControllerScript.ApplyKnockback(finalForce);
                
                _playerHealthScript.Damaged(1);
            }
        }
    }
}
