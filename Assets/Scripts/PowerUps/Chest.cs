using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _coinsReward = 10;
    [SerializeField] private float _detectionRadius = 2.5f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Visuals & Audio")]
    [SerializeField] private GameObject _coinVFXPrefab; // Prefab de las partículas
    [SerializeField] private Transform _vfxSpawnPoint;   // Lugar de donde saldrán las monedas
    [SerializeField] private SoundType _openSound = SoundType.Chest; // Ajusta a tu enum

    private Animator _animator;
    private bool _isOpened = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Si ya se abrió, no seguimos comprobando nada
        if (_isOpened) return;

        // Comprobamos si el jugador está dentro del radio de detección
        bool playerNear = Physics.CheckSphere(transform.position, _detectionRadius, _playerLayer);

        if (playerNear)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        _isOpened = true;

        // 1. Activar animación (Asegúrate de tener un Trigger llamado "Open" en tu Animator)
        if (_animator != null)
        {
            _animator.SetTrigger("Open");
        }

        // 2. Sonido
        SoundManager.PlaySound(_openSound, 1f);

        // 3. Crear el efecto visual (VFX) de las monedas
        if (_coinVFXPrefab != null)
        {
            Transform spawnPoint = _vfxSpawnPoint != null ? _vfxSpawnPoint : transform;
            GameObject vfx = Instantiate(_coinVFXPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Destruimos el objeto del VFX después de 3 segundos para no llenar la memoria
            Destroy(vfx, 3f); 
        }

        // 4. Añadir las monedas al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(_coinsReward);
        }
    }

    // Dibujar el radio en el editor para que puedas ajustarlo visualmente
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}