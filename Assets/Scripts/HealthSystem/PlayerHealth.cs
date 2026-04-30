using UnityEngine;

public class PlayerHealth : HealthSystem
{
    public float playerMaxHealth = 4;
    [SerializeField] private Transform spawnPoint;

    void Start()
    {
        InitialHealth(playerMaxHealth);
        UpdateVisuals();
    }

    
    public void Damaged(float damage)
    {
        TakeDamage(damage);

        UpdateVisuals();

        if(currentHealth <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Respawn();

    }

    private void UpdateVisuals()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHealthUI(currentHealth);
        }
    }

    public void Respawn()
    {
        currentHealth = playerMaxHealth;
        if(spawnPoint != null)
        {
            CharacterController _playerScript = GetComponent<CharacterController>();
            if(_playerScript != null)
            {
                _playerScript.enabled = false;
            }

            transform.position = spawnPoint.position;

            if(_playerScript != null)
            {
                _playerScript.enabled = true;
            }
        }
        UpdateVisuals();
    }

    public void ReturnToCheckpoint()
    {
        if(spawnPoint != null)
        {
            CharacterController _playerScript = GetComponent<CharacterController>();
            if(_playerScript != null)
            {
                _playerScript.enabled = false;
            }

            transform.position = spawnPoint.position;

            if(_playerScript != null)
            {
                _playerScript.enabled = true;
            }
        }
    }

    public void UpdateSpawnPoint(Transform newSpawn)
    {
        spawnPoint = newSpawn;
    }
}