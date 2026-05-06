using UnityEngine;

public class FootStepsLogic : MonoBehaviour
{
    public AudioSource audioSource;
    //public AudioClip[] footstepClips;
    
    [Header("Configuración de Ritmo")]
    public float baseStepSpeed = 0.5f; // Tiempo entre pasos al caminar
    public float sprintStepSpeed = 0.3f; // Tiempo entre pasos al correr
    
    private float footstepTimer = 0f;
    private CharacterController controller; // O tu script de movimiento

    [Header("Sonidos Únicos")]
    public AudioClip metalClip;    // Arrastra aquí tu clip de metal
    public AudioClip cementoClip;  // Arrastra aquí tu clip de cemento

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. Verificar si el personaje está en el suelo y moviéndose
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            // 2. Determinar la velocidad actual del paso
            float currentStepSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintStepSpeed : baseStepSpeed;

            // 3. Lógica del temporizador
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                PlayFootstepSound();
                footstepTimer = currentStepSpeed;
            }
        }
        else
        {
            // Resetear el timer si se detiene para que el primer paso sea instantáneo al volver a movernos
            footstepTimer = 0; 
        }
    }

    void PlayFootstepSound()
    {
        RaycastHit hit;
        // Lanzamos el rayo hacia abajo (1.2 metros de distancia)
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f))
        {
            AudioClip sonidoElegido = null;

            // Comparamos el Tag del suelo
            if (hit.collider.CompareTag("Metal"))
            {
                sonidoElegido = metalClip;
            }
            else 
            {
                // Si no es metal, asumimos que es cemento (suelo por defecto)
                sonidoElegido = cementoClip;
            }

            // Reproducir
            if (sonidoElegido != null)
            {
                // Variamos el pitch un poquito para que no canse el oído
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(sonidoElegido);
            }
        }
    }
}
