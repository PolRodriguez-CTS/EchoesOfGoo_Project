using UnityEngine;

public class CajaDaño : MonoBehaviour
{
    private float cajaDamage = 20f;
    
    void Update()
    {
        
    }

    void OnTriggerEnter (Collider other)
    {
        Debug.Log("Algo entró al trigger");
        if(other.gameObject.tag == "Player")
        {
            PlayerHealth _playerHealthScript = other.gameObject.GetComponent<PlayerHealth>();
            
            if (_playerHealthScript != null)
            {
                _playerHealthScript.Damaged(cajaDamage);
            }
            
        }
    }
}
