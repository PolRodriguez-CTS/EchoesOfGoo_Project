using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    private Animator animator;
    public DoorManager manager; // ASIGNAR EN EL INSPECTOR
    private bool _isPressed = false;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject _vfxEffect; // Arrastra aquí tus partículas, luces o brillos

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (_vfxEffect != null) 
        {
            _vfxEffect.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !_isPressed)
        {
            _isPressed = true;
            animator.SetBool("isPressed", true);
            
            // 1. Lógica de la puerta
            if(manager != null) manager.PlateActivated(); 

            // 2. ACTIVAR EFECTO
            if (_vfxEffect != null) 
            {
                _vfxEffect.SetActive(true);
            }
        }
    }
/*
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") && _isPressed)
        {
            _isPressed = false;
            animator.SetBool("isPressed", false);
            manager.PlateDeactivated(); // Avisamos que se liberó
        }
    }
*/
}