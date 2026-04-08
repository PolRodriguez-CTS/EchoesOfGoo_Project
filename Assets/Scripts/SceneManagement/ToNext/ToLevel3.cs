using UnityEngine;

public class ToLevel3 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Mazmorra_2Manager.Instance.NextLevel();
    }
}
