using UnityEngine;

public class ControlVisualMecanica : MonoBehaviour
{
    [SerializeField] private GameObject visualTeclado;
    [SerializeField] private GameObject visualMando;

    private void OnEnable()
    {
        // Al encenderse el panel, comprueba qué se está usando ahora mismo
        ActualizarVisual(InputDeviceDetector.Instance.dispositivoActivo);

        // Y se suscribe por si el jugador cambia de mando a teclado mientras el cartel está en pantalla
        InputDeviceDetector.Instance.OnDeviceChanged += ActualizarVisual;
    }

    private void OnDisable()
    {
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.OnDeviceChanged -= ActualizarVisual;
        }
    }

    private void ActualizarVisual(InputDeviceDetector.DispositivoActual dispositivo)
    {
        if (dispositivo == InputDeviceDetector.DispositivoActual.Mando)
        {
            visualMando.SetActive(true);
            visualTeclado.SetActive(false);
        }
        else
        {
            visualMando.SetActive(false);
            visualTeclado.SetActive(true);
        }
    }
}