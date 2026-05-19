using UnityEngine;
using UnityEngine.InputSystem; // Usamos el nuevo sistema

public class InputDeviceDetector : MonoBehaviour
{
    public static InputDeviceDetector Instance { get; private set; }

    public enum DispositivoActual { TecladoRaton, Mando }
    public DispositivoActual dispositivoActivo { get; private set; }

    public System.Action<DispositivoActual> OnDeviceChanged;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 1. DETECTAR SI SE USA EL MANDO (GAMEPAD)
        if (Gamepad.current != null)
        {
            // ¿Se ha tocado CUALQUIER elemento del mando en este frame?
            if (Gamepad.current.wasUpdatedThisFrame)
            {
                // Un pequeño filtrado: a veces los sticks tienen "drift" (ruido) y envían micro-movimientos.
                // Nos aseguramos de que realmente se está pulsando un botón o moviendo el stick con intención.
                if (Gamepad.current.buttonSouth.isPressed || Gamepad.current.buttonEast.isPressed ||
                    Gamepad.current.buttonWest.isPressed || Gamepad.current.buttonNorth.isPressed ||
                    Gamepad.current.leftStick.ReadValue().magnitude > 0.2f || 
                    Gamepad.current.rightStick.ReadValue().magnitude > 0.2f ||
                    Gamepad.current.leftTrigger.isPressed || Gamepad.current.rightTrigger.isPressed)
                {
                    CambiarDispositivo(DispositivoActual.Mando);
                    return; // Salimos del Update en este frame
                }
            }
        }

        // 2. DETECTAR SI SE USA TECLADO O RATÓN
        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
        {
            CambiarDispositivo(DispositivoActual.TecladoRaton);
        }
        else if (Mouse.current != null)
        {
            // Si el jugador mueve el ratón o hace clic en los botones principales
            if (Mouse.current.delta.ReadValue().magnitude > 0.1f || 
                Mouse.current.leftButton.isPressed || 
                Mouse.current.rightButton.isPressed)
            {
                CambiarDispositivo(DispositivoActual.TecladoRaton);
            }
        }
    }

    private void CambiarDispositivo(DispositivoActual nuevoDispositivo)
    {
        if (nuevoDispositivo != dispositivoActivo)
        {
            dispositivoActivo = nuevoDispositivo;
            OnDeviceChanged?.Invoke(dispositivoActivo);
            Debug.Log($"[DETECTOR] Dispositivo cambiado a: {dispositivoActivo}");
        }
    }
}