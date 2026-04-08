using UnityEngine;

public class ToLevel2 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Mazmorra_1Manager.Instance.NextLevel();
        }
    }
}
