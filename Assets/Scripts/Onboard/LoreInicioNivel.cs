using UnityEngine;

public class LoreInicioNivel : MonoBehaviour
{
    [SerializeField] private int idDelLoreEnElUIManager = 0;

    private void Start()
    {
        // En cuanto la escena se carga y se ejecuta el Start, lanzamos el panel con pausa
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarPanelLoreConPausa(idDelLoreEnElUIManager);
        }
    }
}