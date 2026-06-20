using UnityEngine;
using UnityEngine.Video; 
using UnityEngine.InputSystem;

[RequireComponent(typeof(VideoPlayer))]
public class VideoCinematicaTrigger : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private bool cinematicaTerminada = false;

    [Header("Referencias Visuales")]
    [Tooltip("La RawImage o el objeto que muestra el vídeo.")]
    [SerializeField] private GameObject objetoVisualDelVideo;
    
    [Tooltip("Opcional: Si el vídeo usa una Render Texture, arrástrala aquí para limpiarla al acabar y que no se quede congelada.")]
    [SerializeField] private RenderTexture texturaDelVideo;

    [Header("Controles del Personaje")]
    [Tooltip("Arrastra aquí a tu personaje (el que tiene el script de movimiento/inputs) para obligarle a despertar.")]
    [SerializeField] private GameObject personaje;

    [Header("Configuración del Tutorial")]
    [SerializeField] private bool esImagenExplicativa;
    [SerializeField] private int idMecanicaOLore;
    [SerializeField] private float duracionUI = 5f;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        if (videoPlayer != null) videoPlayer.loopPointReached += OnVideoTerminado;
        cinematicaTerminada = false;
    }

    private void OnDisable()
    {
        if (videoPlayer != null) videoPlayer.loopPointReached -= OnVideoTerminado;
    }

    private void Update()
    {
        if (cinematicaTerminada) return;

        bool skipTeclado = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool skipMando = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (skipTeclado || skipMando)
        {
            Debug.Log("[SKIP] Forzando salida de la cinemática.");
            
            // Paramos el motor del vídeo por completo
            if (videoPlayer != null)
            {
                videoPlayer.targetTexture = null; // Desvinculamos la textura para romper el renderizado
                videoPlayer.Stop();
            }
            
            OnVideoTerminado(videoPlayer);
        }
    }

    private void OnVideoTerminado(VideoPlayer source)
    {
        if (cinematicaTerminada) return;
        cinematicaTerminada = true;

        // 1. LIMPIEZA DE TEXTURA (Evita el fotograma congelado)
        if (texturaDelVideo != null)
        {
            texturaDelVideo.Release(); // Vacía la memoria de la textura en la gráfica
        }

        // 2. APAGAR PANTALLA
        if (objetoVisualDelVideo != null)
        {
            objetoVisualDelVideo.SetActive(false);
        }

        // 3. DESPERTAR AL PERSONAJE Y SUSS INPUTS
        if (personaje != null)
        {
            // Forzamos a que el objeto del personaje esté activo
            personaje.SetActive(true);
            
            // Si tienes el script PlayerInput del nuevo Input System en el personaje, lo reactivamos
            PlayerInput pInput = personaje.GetComponent<PlayerInput>();
            if (pInput != null) pInput.ActivateInput();
            
            // Habilitamos cualquier script de movimiento clásico que pudiera estar dormido
            MonoBehaviour[] scripts = personaje.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = true;
            }
        }

        // 4. ASEGURAR TIEMPO REAL
        Time.timeScale = 1f;

        // 5. MOSTRAR TUTORIAL
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarPanelTemporal(idMecanicaOLore, duracionUI, esImagenExplicativa);
        }

        enabled = false; 
    }
}