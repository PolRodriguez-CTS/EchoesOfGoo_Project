using UnityEngine;
using System.Collections.Generic;

public class EnemyAreaCheck : MonoBehaviour
{
    public Door doorScript; // Tu script de la puerta
    
    // Lista para rastrear enemigos vivos en la zona
    private List<GameObject> enemiesInRange = new List<GameObject>();

    void Update()
    {
        // Limpiamos la lista de enemigos que hayan sido destruidos
        enemiesInRange.RemoveAll(enemy => enemy == null);

        // Si la lista se vacía, abrimos la puerta
        if (enemiesInRange.Count == 0)
        {
            doorScript.OpenAnimation();
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