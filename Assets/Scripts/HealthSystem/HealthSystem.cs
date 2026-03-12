using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    protected float currentHealth;
    protected float maxHealth;

    void Start()
    {
        InitialHealth(maxHealth);
    }

    void Update()
    {
        
    }

    public void InitialHealth(float maxHealth)
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Debug.Log(this + "ha muerto");
    }
}