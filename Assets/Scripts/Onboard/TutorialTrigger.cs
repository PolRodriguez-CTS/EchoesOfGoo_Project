using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public enum TipoTutorial { Controles, MiniTutorial, Dialogo, Lore }
    
    [Header("Configuración del Trigger")]
    [SerializeField] private TipoTutorial tipo;
    [SerializeField] private float duracion = 5f;

    [Header("Contenido (Rellenar según el tipo)")]
    [SerializeField] private Sprite imagenParaMostrar;
    [TextArea(3, 5)] [SerializeField] private string textoParaMostrar;

    private void OnTriggerEnter(Collider other) // Usa OnTriggerEnter2D si es un juego 2D
    {
        if (other.CompareTag("Player"))
        {
            switch (tipo)
            {
                case TipoTutorial.Controles:
                    UIManager.Instance.MostrarControles(duracion);
                    break;
                case TipoTutorial.MiniTutorial:
                    UIManager.Instance.MostrarMiniTutorial(imagenParaMostrar, duracion);
                    break;
                case TipoTutorial.Dialogo:
                    UIManager.Instance.MostrarDialogo(textoParaMostrar);
                    break;
                case TipoTutorial.Lore:
                    UIManager.Instance.MostrarLore(textoParaMostrar);
                    break;
            }

            // Destruimos este trigger para que solo salte una vez
            Destroy(gameObject);
        }
    }
}
