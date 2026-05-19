using UnityEngine;
using System.Collections;
using UnityEngine.UI; // IMPRESCINDIBLE para usar el texto Legacy (Text)

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("1. Catálogo de Controles (Paneles)")]
    [SerializeField] private GameObject[] panelesControles; 

    [Header("2. Catálogo de Imágenes Explicativas (Paneles)")]
    [SerializeField] private GameObject[] panelesImagenesExplicativas; 

    [Header("3. Catálogo de Lore (Paneles Independientes)")]
    // Aquí arrastrarás tus dos paneles de Lore desde el Canvas Core
    [SerializeField] private GameObject[] panelesLore; 

    private Coroutine UIWaitCoroutine;
    private GameObject panelActivoTemporal; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- GESTIÓN DE CONTROLES E IMÁGENES (Paneles temporales sin pausa) ---
    public void MostrarPanelTemporal(int id, float tiempo, bool esImagenExplicativa)
    {
        GameObject[] catalogo = esImagenExplicativa ? panelesImagenesExplicativas : panelesControles;

        if (id < 0 || id >= catalogo.Length)
        {
            Debug.LogError($"El ID {id} no existe en el catálogo seleccionado.");
            return;
        }

        if (UIWaitCoroutine != null) 
        {
            StopCoroutine(UIWaitCoroutine);
            if (panelActivoTemporal != null) panelActivoTemporal.SetActive(false);
        }

        GameObject panelAActivar = catalogo[id];
        UIWaitCoroutine = StartCoroutine(PanelTemporalCoroutine(panelAActivar, tiempo));
    }

    private IEnumerator PanelTemporalCoroutine(GameObject panel, float tiempo)
    {
        panelActivoTemporal = panel;
        panelActivoTemporal.SetActive(true);
        yield return new WaitForSeconds(tiempo);
        panelActivoTemporal.SetActive(false);
        panelActivoTemporal = null;
    }

    // --- GESTIÓN DE LORE (Pausa el juego y enciende el panel elegido) ---
    public void MostrarLorePorID(int idLore, float tiempoDuracion)
    {
        if (idLore < 0 || idLore >= panelesLore.Length)
        {
            Debug.LogError($"El ID de Lore {idLore} no existe en el UIManager.");
            return;
        }

        // Activamos el panel de lore específico (que ya tiene su texto clásico dentro)
        GameObject panelLoreAActivar = panelesLore[idLore];
        panelLoreAActivar.SetActive(true);

        // Pausamos el juego por completo para que no se mueva el personaje
        Time.timeScale = 0f;

        // Iniciamos la cuenta atrás en tiempo real
        StartCoroutine(CerrarLoreTrasTiempo(panelLoreAActivar, tiempoDuracion));
    }

    private IEnumerator CerrarLoreTrasTiempo(GameObject panelLore, float tiempo)
    {
        // Espera en tiempo real (ignora el Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(tiempo);
        
        // Apagamos el panel y devolvemos el juego a la normalidad
        panelLore.SetActive(false);
        Time.timeScale = 1f; 
    }
}