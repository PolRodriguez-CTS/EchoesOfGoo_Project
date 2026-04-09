using UnityEngine;

public class EnemyHealth : HealthSystem
{
public float EnemyMaxHealth = 3;
private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        InitialHealth(EnemyMaxHealth);
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
        Destroy(gameObject);
        //animator.SetTrigger("isDead");
    }
}