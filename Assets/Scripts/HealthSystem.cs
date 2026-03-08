using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    private float currentHealth;
    private float maxHealth;

    void Start()
    {
        Debug.Log("gameObject.name");
    }

    void Update()
    {
        
    }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }
}
