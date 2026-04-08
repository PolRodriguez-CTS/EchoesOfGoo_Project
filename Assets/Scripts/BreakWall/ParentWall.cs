using UnityEngine;

public class ParentWall : MonoBehaviour
{
   void OnTriggerEnter(Collider collider)
{
    if (collider.CompareTag("Player"))
    {
        // Busca todos los Rigidbodys en los hijos y los libera
        Rigidbody[] trozos = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in trozos)
        {
            rb.isKinematic = false;
        }
    }
}
}
