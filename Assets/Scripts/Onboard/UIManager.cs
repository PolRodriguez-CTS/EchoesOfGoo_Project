using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneles de la UI")]
    [SerializeField] private GameObject panelControles;
    [SerializeField] private GameObject panelTutorial;
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private GameObject panelLore;

    [Header("Componentes del Panel Tutorial")]
    [SerializeField] private Image imagenTutorial;

    [Header("Componentes del Panel Diálogo/Lore")]
    [SerializeField] private Text textoDialogo;
    [SerializeField] private Text textoLore;

    private Coroutine controlesCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 1. CONTROL TEMPORAL (Desaparece solo)
    public void MostrarControles(float tiempo)
    {
        if (controlesCoroutine != null) StopCoroutine(controlesCoroutine);
        controlesCoroutine = StartCoroutine(ControlesTemporalCoroutine(tiempo));
    }

    private IEnumerator ControlesTemporalCoroutine(float tiempo)
    {
        panelControles.SetActive(true);
        yield return new WaitForSeconds(tiempo);
        panelControles.SetActive(false);
    }

    // 2. MINI TUTORIAL CON IMAGEN (Desaparece solo o por tiempo)
    public void MostrarMiniTutorial(Sprite nuevaImagen, float tiempo)
    {
        imagenTutorial.sprite = nuevaImagen;
        panelTutorial.SetActive(true);
        StartCoroutine(DesactivarPanelDespuesDeTiempo(panelTutorial, tiempo));
    }

    private IEnumerator DesactivarPanelDespuesDeTiempo(GameObject panel, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        panel.SetActive(false);
    }

    // 3. DIÁLOGOS DE PERSONAJES
    public void MostrarDialogo(string texto)
    {
        textoDialogo.text = texto;
        panelDialogo.SetActive(true);
        // Aquí podrías bloquear el movimiento o dejar que desaparezca al pulsar un botón
    }

    public void CerrarDialogo() => panelDialogo.SetActive(false);

    // 4. LORE CON BLOQUEO DE MOVIMIENTO
    public void MostrarLore(string texto)
    {
        textoLore.text = texto;
        panelLore.SetActive(true);

        // Bloqueamos el tiempo del juego o desactivamos el input del jugador
        Time.timeScale = 0f; // Pausa el juego (fácil y rápido)
        
        // Si usas el nuevo Input System, es mejor hacer: 
        // tuPlayerInput.DeactivateInput();
    }

    public void CerrarLore()
    {
        panelLore.SetActive(false);
        Time.timeScale = 1f; // Reanudamos el juego
    }

    public void MostrarLoreTemporal(string texto, float tiempoDeEspera)
    {
    textoLore.text = texto;
    panelLore.SetActive(true);

    // Bloqueamos el movimiento desactivando el tiempo del juego
    Time.timeScale = 0f; 

    // Iniciamos la corrutina que cuenta el tiempo en "la vida real"
    StartCoroutine(CerrarLoreTrasTiempo(tiempoDeEspera));
    }

    private IEnumerator CerrarLoreTrasTiempo(float tiempo)
    {
        // Usamos Realtime para que ignore el Time.timeScale = 0
        yield return new WaitForSecondsRealtime(tiempo);
        
        // Al pasar el tiempo, desactivamos el panel y devolvemos el juego a la normalidad
        panelLore.SetActive(false);
        Time.timeScale = 1f; 
    }
}