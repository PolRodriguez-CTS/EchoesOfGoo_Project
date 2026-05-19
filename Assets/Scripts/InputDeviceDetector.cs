using UnityEngine;
using UnityEngine.InputSystem; // Usamos el nuevo sistema

public class InputDeviceDetector : MonoBehaviour
{
    public static InputDeviceDetector Instance { get; private set; }

    public enum DispositivoActual { TecladoRaton, Mando }
    public DispositivoActual dispositivoActivo { get; private set; }

    public System.Action<DispositivoActual> OnDeviceChanged;

    // ⚠️ REEMPLAZA "MisControles" por el nombre de tu clase generada del Input System
    private MisControles controlesGlobales; 

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Inicializamos tus controles aquí en la escena Core
            controlesGlobales = new MisControles();
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (controlesGlobales != null) controlesGlobales.Enable();
        
        // El truco definitivo: Nos suscribimos al evento global de la Input System 
        // que detecta CUALQUIER acción de cualquier dispositivo en el juego
        InputSystem.onActionChange += AlDetectarAccionGlobal;
    }

    private void OnDisable()
    {
        if (controlesGlobales != null) controlesGlobales.Disable();
        InputSystem.onActionChange -= AlDetectarAccionGlobal;
    }

    private void AlDetectarAccionGlobal(object obj, InputActionChange cambio)
    {
        // Solo nos interesa cuando una acción se ejecuta (el jugador pulsa algo)
        if (cambio == InputActionChange.ActionStarted)
        {
            InputAction accionEjecutada = obj as InputAction;
            
            if (accionEjecutada != null && accionEjecutada.activeControl != null)
            {
                // Miramos qué dispositivo físico ha activado esa acción
                InputDevice dispositivoFisico = accionEjecutada.activeControl.device;

                DispositivoActual nuevoDispositivo = dispositivoActivo;

                // Si el dispositivo es un Gamepad o Joystick...
                if (dispositivoFisico is Gamepad || dispositivoFisico is Joystick)
                {
                    nuevoDispositivo = DispositivoActual.Mando;
                }
                else // Si es teclado, ratón, etc.
                {
                    nuevoDispositivo = DispositivoActual.TecladoRaton;
                }

                // Si ha cambiado respecto al que teníamos, actualizamos la UI
                if (nuevoDispositivo != dispositivoActivo)
                {
                    dispositivoActivo = nuevoDispositivo;
                    OnDeviceChanged?.Invoke(dispositivoActivo);
                    Debug.Log($"Dispositivo cambiado a: {dispositivoActivo}");
                }
            }
        }
    }
}