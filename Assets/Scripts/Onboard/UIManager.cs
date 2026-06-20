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

    [Header("Configuración del Fade")]
    [SerializeField] private float duracionFadeIn = 0.5f;
    [SerializeField] private float duracionFadeOut = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- GESTIÓN DE CONTROLES E IMÁGENES (Paneles temporales sin pausa) ---
    // --- GESTIÓN DE CONTROLES E IMÁGENES (Modificado para permitir Ataque + Stun) ---
    public void MostrarPanelTemporal(int id, float tiempo, bool esImagenExplicativa)
    {
        // 🌟 CONFIGURA AQUÍ LOS IDS REALES DE TU INSPECTOR:
        int idAtaque = 3; // Cambia por el ID real de tu panel de Ataque
        int idStun = 4;   // Cambia por el ID real de tu panel de Stun
        bool catalogoDeLosDos = false; // Pon 'true' si están en Imágenes Explicativas, o 'false' si son de Controles

        GameObject[] catalogo = esImagenExplicativa ? panelesImagenesExplicativas : panelesControles;

        if (id < 0 || id >= catalogo.Length)
        {
            Debug.LogError($"El ID {id} no existe en el catálogo seleccionado.");
            return;
        }

        // 🌟 REGLA DE EXCEPCIÓN INTEGRADA:
        // Si el panel que vamos a encender AHORA es Ataque o Stun, y el que YA estaba puesto era su compañero,
        // nos SALTAMOS el apagado de seguridad para que convivan en pantalla.
        bool esCombinacionEspecial = (!esImagenExplicativa && !catalogoDeLosDos) && 
                                     ((id == idAtaque && panelActivoTemporal == catalogo[idStun]) || 
                                      (id == idStun && panelActivoTemporal == catalogo[idAtaque]));

        if (UIWaitCoroutine != null && !esCombinacionEspecial) 
        {
            StopCoroutine(UIWaitCoroutine); // Solo frenamos el temporizador si NO es la combinación especial
            
            if (panelActivoTemporal != null) 
            {
                panelActivoTemporal.SetActive(false); // Apagamos el panel viejo normal
            }
        }

        // Guardamos la referencia del nuevo panel que se enciende
        panelActivoTemporal = catalogo[id];

        GameObject panelAActivar = catalogo[id];
        if (panelAActivar.transform.parent != null)
        {
            panelAActivar.transform.parent.gameObject.SetActive(true);
        }

        panelAActivar.SetActive(true);

        UIFadeHandler fadeHelper = panelAActivar.GetComponent<UIFadeHandler>();
        if (fadeHelper != null)
        {
            fadeHelper.Aparecer(duracionFadeIn);
        }
        
        // Si es la combinación especial, lanzamos una rutina que apague este elemento en concreto, 
        // evitando sobreescribir la rutina global para que el otro panel termine su tiempo de forma independiente.
        if (esCombinacionEspecial)
        {
            StartCoroutine(PanelTemporalCoroutine(panelAActivar, tiempo));
        }
        else
        {
            UIWaitCoroutine = StartCoroutine(PanelTemporalCoroutine(panelAActivar, tiempo));
        }
    }

    private IEnumerator PanelTemporalCoroutine(GameObject panel, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        panelActivoTemporal = null;

        // Buscamos el ayudante de fade
        UIFadeHandler fadeHelper = panel.GetComponent<UIFadeHandler>();
        if (fadeHelper != null)
        {
            // Le pedimos que desaparezca
            fadeHelper.Desaparecer(duracionFadeOut);
            
            // El propio UIFadeHandler se encargará de hacer el SetActive(false) al terminar el fade
        }
        else
        {
            // Si no tiene fade, lo apagamos de golpe como antes
            panel.SetActive(false);
        }
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

    private GameObject panelLorePausado;

    public void MostrarPanelLoreConPausa(int id)
    {
        // 🕵️‍♂️ ¡EL CHIVATO! Esto nos dirá en la consola exactamente cuándo se ejecuta esto
        Debug.LogWarning($"[RUSTREO PAUSA] ¡Ojo! Alguien ha llamado a MostrarPanelLoreConPausa con el ID {id}. " +
                        $"Escena Activa: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        if (id < 0 || id >= panelesLore.Length)
        {
            Debug.LogError($"El ID de lore {id} no existe en el catálogo.");
            return;
        }

        if (panelesLore[id].transform.parent != null)
        {
            panelesLore[id].transform.parent.gameObject.SetActive(true);
        }

        panelLorePausado = panelesLore[id];
        panelLorePausado.SetActive(true);

        UIFadeHandler fade = panelLorePausado.GetComponent<UIFadeHandler>();
        if (fade != null) fade.Aparecer(0.3f);

        //Time.timeScale = 0f;
    }

    public void DespausarYQuitarLore()
    {
        if (panelLorePausado != null)
        {
            UIFadeHandler fade = panelLorePausado.GetComponent<UIFadeHandler>();
            if (fade != null)
            {
                fade.Desaparecer(0.3f); // El propio fade lo desactivará al terminar
            }
            else
            {
                panelLorePausado.SetActive(false);
            }
            
            panelLorePausado = null;
        }

        // 🌟 EL PASO 3 DE SEGURIDAD AQUÍ:
        // Buscamos el retardador que congeló al jugador y le pedimos que lo desbloquee
        RetardadorLoreInicio retardador = FindFirstObjectByType<RetardadorLoreInicio>();
        if (retardador != null)
        {
            retardador.DesbloquearAlJugador();
            Debug.Log("[UIManager] Lore cerrado. Solicitando desbloqueo de inputs al jugador.");
        }
        else
        {
            // Por si acaso estás en un nivel donde no hay retardador, aseguramos el tiempo normal
            Time.timeScale = 1f;
        }
    }

    private IEnumerator CerrarLoreTrasTiempo(GameObject panelLore, float tiempo)
    {
        // Espera en tiempo real (ignora el Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(tiempo);
        
        // Apagamos el panel y devolvemos el juego a la normalidad
        panelLore.SetActive(false);
        Time.timeScale = 1f; 
    }

    private void OnDestroy()
    {
        // 🌟 SEGURO ANTIBLOQUEO: Si el UIManager se destruye para cambiar de nivel,
        // nos aseguramos al 100% de que el tiempo vuelve a 1 para la pantalla de carga.
        Time.timeScale = 1f;
        Debug.Log("[UIManager] Escena destruida. Forzando TimeScale a 1f.");
    }
}