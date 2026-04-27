using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _pushForce = 10f;
    [SerializeField] private float _verticalLift = 2f;
    [SerializeField] private float _activationDelay = 1.5f;
    private Animator _animator;
    private bool _isPlayerOver = false;
    private PlayerController _playerRef;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // 1. El trigger detecta que el jugador está "encima" esperando
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerOver = true;
            _playerRef = other.GetComponent<PlayerController>();
            
            // Disparamos la animación de aviso o preparación
            _animator.SetTrigger("PrepareSpikes"); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerOver = false;
            _playerRef = null;
        }
    }

    // 2. ESTA FUNCIÓN LA LLAMA EL EVENTO DE ANIMACIÓN
    public void ActivarPinchos()
    {
        // Solo empujamos si el jugador sigue encima cuando los pinchos salen
        if (_isPlayerOver && _playerRef != null)
        {
            // Calculamos dirección (hacia arriba y ligeramente hacia afuera)
            Vector3 pushDir = (_playerRef.transform.position - transform.position);
            pushDir.y = 0;
            pushDir = pushDir.normalized;

            Vector3 finalForce = (pushDir * _pushForce) + (Vector3.up * _verticalLift);
            
            _playerRef.ApplyKnockback(finalForce);

            // Opcional: Daño
            _playerRef.GetComponent<PlayerHealth>().Damaged(1);
        }
    }
}