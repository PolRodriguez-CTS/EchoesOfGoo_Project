using UnityEngine;
using UnityEngine.InputSystem; // Nuevo sistema

public class CerrarLorePorBoton : MonoBehaviour
{
    private void Update()
    {
        // Como el juego está en Time.timeScale = 0, el Update normal sigue funcionando.
        // Comprobamos si se pulsa el Espacio en teclado o el botón sur (A/X) en el mando
        if ((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.DespausarYQuitarLore();
            }
        }
    }
}