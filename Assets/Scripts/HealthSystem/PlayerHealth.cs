using UnityEngine;

public class PlayerHealth : HealthSystem
{
    public float playerMaxHealth = 4;
    [SerializeField] private Transform spawnPoint;

    void Start()
    {
        InitialHealth(playerMaxHealth);
    }

    void Update()
    {
        //Debug.Log("Vida player:" + currentHealth);
    }

    
    public void Damaged(float damage)
    {
        TakeDamage(damage);

        if(currentHealth <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Respawn();

        FullHeal();
    }

    public void Respawn()
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

    public void FullHeal()
    {
        currentHealth = playerMaxHealth;
    }
}