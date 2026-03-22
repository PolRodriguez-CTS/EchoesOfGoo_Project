using UnityEngine;

public class PlayerHealth : HealthSystem
{
    private float playerMaxHealth = 100;
    void Start()
    {
        InitialHealth(playerMaxHealth);
    }

    void Update()
    {
        Debug.Log("Vida player:" + currentHealth);
    }

    
    public void Damaged(float damage)
    {
        TakeDamage(damage);
    }
}