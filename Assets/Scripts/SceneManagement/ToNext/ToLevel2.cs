using UnityEngine;

public class ToLevel2 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Mazmorra_1Manager.Instance.NextLevel();
    }
}