using UnityEngine;

public class Axe : MonoBehaviour
{
    void Awake()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {
        PlayerHealth playerScript = GetComponent<PlayerHealth>();
        playerScript.Damaged(1);
    }
}
