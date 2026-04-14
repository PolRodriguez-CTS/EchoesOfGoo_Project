using UnityEngine;
using System.Collections;

public class PlayerHealth : HealthSystem
{
    public float playerMaxHealth = 4;
    [SerializeField] private Transform spawnPoint;
    private float knockDuration;
    private Vector3 knockForce;

    void Start()
    {
        InitialHealth(playerMaxHealth);
        UpdateVisuals();
    }

    
    public void Damaged(float damage)
    {
        StartCoroutine(ApplyKnockback(knockForce, knockDuration));

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

    private IEnumerator ApplyKnockback(Vector3 force, float duration)
    {
        float elapsed = 0;
        while(elapsed < duration)
        {
            Vector3 knockForce = Vector3.Lerp(force, Vector3.zero, elapsed / duration);
            transform.position += knockForce * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}