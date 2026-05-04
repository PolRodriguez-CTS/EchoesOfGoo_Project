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

    public float GetCurrentHealth() { return currentHealth; }

    public void Heal(float amount)
    {
        currentHealth += amount;

        // Si la vida actual supera el máximo, la igualamos al máximo
        if (currentHealth > playerMaxHealth)
        {
            currentHealth = playerMaxHealth;
        }

        Debug.Log("Curado. Vida actual: " + currentHealth);
    }

    public void UpdateSpawnPoint(Transform newSpawn)
    {
        spawnPoint = newSpawn;
    }
}