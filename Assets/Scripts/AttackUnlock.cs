using UnityEngine;

public class AttackUnlock : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //LocalLevelLock.CanAttack = true;
        }
    }
}