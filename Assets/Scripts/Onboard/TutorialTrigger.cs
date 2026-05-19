using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public enum TipoDeTrigger { Control, ImagenExplicativa, Lore }

    [Header("Tipo de Contenido")]
    [SerializeField] private TipoDeTrigger tipo;

    [Header("Configuración")]
    [Tooltip("El número de ID según el orden que pusiste en el UIManager")]
    [SerializeField] private int idMecanicaOLore; 
    [Tooltip("Cuánto tiempo se quedará en pantalla")]
    [SerializeField] private float duracion = 5f;

    private void OnTriggerEnter(Collider other) // Cambiar a OnTriggerEnter2D si tu juego es 2D
    {
        if (other.CompareTag("Player"))
        {
            if (UIManager.Instance == null) return;

            switch (tipo)
            {
                case TipoDeTrigger.Control:
                    // Enviamos false porque NO es una imagen explicativa, es un control
                    UIManager.Instance.MostrarPanelTemporal(idMecanicaOLore, duracion, false);
                    break;

                case TipoDeTrigger.ImagenExplicativa:
                    // Enviamos true porque SÍ es una imagen explicativa
                    UIManager.Instance.MostrarPanelTemporal(idMecanicaOLore, duracion, true);
                    break;

                case TipoDeTrigger.Lore:
                    UIManager.Instance.MostrarLorePorID(idMecanicaOLore, duracion);
                    break;
            }

            // Destruimos el trigger para que no se repita
            Destroy(gameObject);
        }
    }
}