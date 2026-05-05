using UnityEngine;
using Unity.Cinemachine; // Asegúrate de tener este namespace
using UnityEngine.InputSystem;

public class CameraFixer : MonoBehaviour
{
    private CinemachineCamera vcam;
    private CinemachineThirdPersonFollow followComponent;

    private InputAction cameraFix;

    void Awake()
    {
        cameraFix = InputSystem.actions["cameraFix"];
    }

    void Start()
    {
        vcam = GetComponent<CinemachineCamera>();
        // Intentamos obtener el componente de seguimiento
        followComponent = vcam.GetComponent<CinemachineThirdPersonFollow>();
    }

    void Update()
    {
        // Si presionas la tecla R, reseteamos la cámara manualmente
        if (cameraFix.WasPressedThisFrame())
        {
            Debug.Log("Hola");
            ResetCameraPosition();
        }
    }

    public void ResetCameraPosition()
    {
        if (followComponent != null)
        {
            // Forzamos el valor de la distancia al que tú quieras
            // Esto "despierta" al componente si se quedó trabado
            float originalDistance = followComponent.CameraDistance;
            followComponent.CameraDistance = 0.1f; // Lo pegamos al personaje
            followComponent.CameraDistance = originalDistance; // Lo devolvemos
            
            Debug.Log("Cámara reseteada forzosamente");
        }
    }
}