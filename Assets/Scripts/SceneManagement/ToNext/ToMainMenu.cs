using UnityEngine;

public class ToMainMenu : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Mazmorra_4Manager.Instance.ReturnMainMenu();
    }
}
