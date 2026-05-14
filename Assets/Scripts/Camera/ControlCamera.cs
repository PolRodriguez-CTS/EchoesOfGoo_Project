using UnityEngine;
using UnityEngine.InputSystem;

public class ControlCamera : MonoBehaviour
{
    [Header("Sensibilidades")]
    public float sensibilidadRaton = 1f;
    public float sensibilidadMando = 25f; // Mucho más alta para compensar

    private Vector2 entradaLook;

    void Start()
    {
        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions.Disable(); // Desactiva todo lo global
        playerInput.actions.FindActionMap("Player").Enable(); // Activa solo el de jugar
    }

    // Esta función la conectas en el evento "Look" del Player Input
    public void OnLook(InputAction.CallbackContext context)
    {
        // Leemos el valor (Vector2)
        entradaLook = context.ReadValue<Vector2>();

        // Si el dispositivo que se mueve es un Gamepad, multiplicamos por su sensibilidad
        if (context.control.device is Gamepad)
        {
            entradaLook *= sensibilidadMando;
        }
        else
        {
            entradaLook *= sensibilidadRaton;
        }
    }

    void Update()
    {
        // Aplicamos el movimiento
        float rotX = entradaLook.x * Time.deltaTime * 10f;
        float rotY = entradaLook.y * Time.deltaTime * 10f;

        // Aquí iría tu lógica de rotación de cámara (transform.Rotate, etc.)
        transform.Rotate(Vector3.up * rotX);
    }
}