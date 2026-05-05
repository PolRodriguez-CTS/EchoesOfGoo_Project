using UnityEngine;
using Unity.Cinemachine;

public class CameraAutoRestorer : MonoBehaviour
{
    private CinemachineCamera vcam;
    private CinemachineThirdPersonFollow followComponent;
    
    [Header("Configuración de Retorno")]
    [Tooltip("Velocidad a la que la cámara intenta recuperar su distancia original")]
    public float recoverySpeed = 5f;
    
    private float targetDistance;

    void Start()
    {
        vcam = GetComponent<CinemachineCamera>();
        followComponent = vcam.GetComponent<CinemachineThirdPersonFollow>();
        
        if (followComponent != null)
        {
            // Guardamos la distancia que configuraste en el Inspector como "ideal"
            targetDistance = followComponent.CameraDistance;
        }
    }

    void LateUpdate() // Usamos LateUpdate para actuar DESPUÉS de que Cinemachine mueva la cámara
    {
        if (followComponent == null) return;

        // Si la cámara está más cerca de lo que debería por culpa de una pared
        // el Deoccluder la mueve, pero nosotros nos aseguramos de que el valor 
        // objetivo siempre intente ser la targetDistance original.
        
        if (followComponent.CameraDistance < targetDistance)
        {
            // Forzamos suavemente a que la distancia objetivo se mantenga
            // Esto ayuda a que el algoritmo de Cinemachine no se "duerma"
            followComponent.CameraDistance = Mathf.Lerp(followComponent.CameraDistance, targetDistance, Time.deltaTime * recoverySpeed);
        }
    }
}