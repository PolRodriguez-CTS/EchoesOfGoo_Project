using UnityEngine;
using System.Collections;

public class WallBreaking : MonoBehaviour
{
    private Rigidbody rigidbodyBox;
    //[SerializeField] float forceAmount = 10f; // Ajusta esto si es muy poco

    void Awake()
    {
        rigidbodyBox = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rigidbodyBox.isKinematic = true;
    }

   void OnTriggerEnter(Collider collider)
{
    if (collider.CompareTag("Player"))
    {
        rigidbodyBox.isKinematic = false;
        rigidbodyBox.WakeUp();

        // 1. Les damos un empujón hacia afuera basado en su propia posición
        // Esto soluciona el problema de los pivotes centrados
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f), 
            Random.Range(0.5f, 1.5f), // Siempre un poco hacia arriba
            Random.Range(-1f, 1f)
        );

        // 2. Aplicamos una fuerza de impulso fuerte
        rigidbodyBox.AddForce(randomDirection * 5f, ForceMode.Impulse);

        // 3. Añadimos rotación (esto es clave para que no caigan "tiesos")
        rigidbodyBox.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
    }
}
}
