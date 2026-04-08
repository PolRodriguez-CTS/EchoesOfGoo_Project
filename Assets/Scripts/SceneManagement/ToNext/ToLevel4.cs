using UnityEngine;

public class ToLevel4 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Mazmorra_3Manager.Instance.NextLevel();
    }
}
