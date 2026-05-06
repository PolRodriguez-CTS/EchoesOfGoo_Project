using UnityEngine;
using System.Collections.Generic;

public class EnemyAreaCheck : MonoBehaviour
{
    public Door doorScript; 
    private List<GameObject> enemiesInRange = new List<GameObject>();

    // ESTA ES LA CLAVE: Una variable para saber si ya abrimos
    private bool hasOpened = false;
    private bool enemiesDetectedOnce = false;

    void Update()
    {
        // 1. Limpiamos la lista
        enemiesInRange.RemoveAll(enemy => enemy == null);

        // 2. Si detectamos enemigos por primera vez, marcamos que la zona está "activa"
        if (enemiesInRange.Count > 0)
        {
            enemiesDetectedOnce = true;
        }

        // 3. Solo intentamos abrir si:
        // - Ya detectamos enemigos antes (para que no se abra sola al empezar el nivel)
        // - La lista está vacía
        // - ¡Y NO la hemos abierto todavía! (hasOpened == false)
        if (enemiesDetectedOnce && enemiesInRange.Count == 0 && !hasOpened)
        {
            doorScript.OpenAnimation();
            hasOpened = true; // Marcamos como abierta para que no entre aquí nunca más
            
            // Opcional: Si ya no necesitas detectar nada más, puedes desactivar el script
            // this.enabled = false; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (!enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            enemiesInRange.Remove(other.gameObject);
        }
    }
}