using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // Por si usas el nuevo Input System

public class RetardadorLoreInicio : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    [Tooltip("Segundos totales desde que se entra al nivel hasta que sale el lore (cuenta el tiempo de carga + tu espera).")]
    [SerializeField] private float tiempoTotalEspera = 5f;

    [Header("Configuración del Lore")]
    [SerializeField] private int idDelLore = 0;

    [Header("Referencia al Personaje")]
    [Tooltip("Arrastra aquí a tu Player para congelar sus movimientos mientras lee.")]
    [SerializeField] private GameObject personaje;

    private void Start()
    {
        StartCoroutine(CronometroLoreSinPausa());
    }

    private IEnumerator CronometroLoreSinPausa()
    {
        // Esperamos el tiempo de cortesía a velocidad normal del juego
        yield return new WaitForSeconds(tiempoTotalEspera);

        Debug.Log($"[LORE] Activando panel {idDelLore} sin pausar el motor del juego.");

        // 1. Bloqueamos al personaje para que no se mueva ni le ataquen
        BloquearPersonaje(true);

        // 2. Le pedimos al UIManager que muestre el panel (Asegúrate de que tu UIManager NO ponga Time.timeScale = 0f)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarPanelLoreConPausa(idDelLore); 
        }
    }

    // Función pública para que el botón de cerrar pueda desbloquear al personaje al acabar
    public void DesbloquearAlJugador()
    {
        BloquearPersonaje(false);
    }

    private void BloquearPersonaje(bool bloquear)
    {
        if (personaje == null) return;

        // Si usas el nuevo Input System:
        PlayerInput pInput = personaje.GetComponent<PlayerInput>();
        if (pInput != null)
        {
            if (bloquear) pInput.DeactivateInput();
            else pInput.ActivateInput();
        }

        // Apagamos/Encendemos los scripts de movimiento para que no se mueva
        MonoBehaviour[] scripts = personaje.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // Evitamos apagarse a sí mismo si estuviera en el player
            if (script != this) 
            {
                script.enabled = !bloquear;
            }
        }
    }
}