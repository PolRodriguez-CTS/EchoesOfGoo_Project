using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Configuración del Control")]
    // Aquí arrastrarás el panel específico de la mecánica desde tu Canvas
    [SerializeField] private GameObject panelDeEstaMecanica; 
    [SerializeField] private float duracion = 4f;

    private void OnTriggerEnter(Collider other) // O OnTriggerEnter2D si es 2D
    {
        if (other.CompareTag("Player"))
        {
            if (panelDeEstaMecanica != null)
            {
                // Le enviamos al UIManager el panel exacto de este trigger
                UIManager.Instance.MostrarControlMecanica(panelDeEstaMecanica, duracion);
            }
            else
            {
                Debug.LogWarning($"¡Ojo! El trigger {gameObject.name} no tiene ningún panel asignado.");
            }

            // Se destruye para que no vuelva a salir
            Destroy(gameObject);
        }
    }
}