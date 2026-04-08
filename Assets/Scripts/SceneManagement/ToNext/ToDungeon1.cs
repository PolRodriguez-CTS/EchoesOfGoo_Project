using UnityEngine;

public class ToDungeon1 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            FabricaManager.Instance.ToDungeon1();
        }
    }
}
