using UnityEngine;

public class LocalLevelLock : MonoBehaviour
{

    [SerializeField] private bool _allowDash = false;
    [SerializeField] private bool _allowAttack = false;

    // Usamos variables estáticas internas para que sean globales y fáciles de leer
    public static bool CanDash { get; private set; } = true;
    public static bool CanAttack { get; private set; } = true;

    void Awake()
    {
        CanDash = true;
        CanAttack = true;
    }

    void OnEnable()
    {
        CanDash = _allowDash;
        CanAttack = _allowAttack;
    }

    void OnDisable()
    {
        // Al cerrar la escena o destruir el objeto, devolvemos los poderes
        CanDash = true;
        CanAttack = true;
    }

    // Función estática para cambiar los valores desde cualquier sitio
    public static void SetPowers(bool dash, bool attack)
    {
        CanDash = dash;
        CanAttack = attack;
    }

    
}
