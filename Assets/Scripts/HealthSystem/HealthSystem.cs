using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    protected float currentHealth;
    protected float maxHealth;

    void Start()
    {
        InitialHealth(maxHealth);
    }

    public void InitialHealth(float maxHealth)
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }
}