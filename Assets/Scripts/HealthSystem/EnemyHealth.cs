using UnityEngine;

public class EnemyHealth : HealthSystem
{
[SerializeField] private float EnemyMaxHealth = 100;
    void Start()
    {
        InitialHealth(EnemyMaxHealth);
    }

    void Update()
    {
        Debug.Log("Vida enemigo:" + currentHealth);
    }

    
    public void Damaged(float damage)
    {
        TakeDamage(damage);
    }
}