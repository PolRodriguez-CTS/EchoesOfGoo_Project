using UnityEngine;

public class PowerRestrictionTrigger : MonoBehaviour
{
    [Header("Configuración al Entrar")]
    [SerializeField] private bool _canDashInside = false;
    [SerializeField] private bool _canAttackInside = false;

    [Header("Configuración al Salir")]
    [SerializeField] private bool _canDashOutside = true;
    [SerializeField] private bool _canAttackOutside = true;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si es el jugador (puedes usar Tag o Componente)
        if (other.CompareTag("Player"))
        {
            LocalLevelLock.SetPowers(_canDashInside, _canAttackInside);
            //Debug.Log("Has entrado en zona restringida");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LocalLevelLock.SetPowers(_canDashOutside, _canAttackOutside);
            Debug.Log("Has salido de la zona restringida");
        }
    }
}
