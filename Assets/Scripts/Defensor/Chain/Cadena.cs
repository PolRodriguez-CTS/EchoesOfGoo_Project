using UnityEngine;
public class Cadena : MonoBehaviour 
{
    public ControladorPrisionero controlador;
    private bool yaSeRompio = false;

    public void RomperTodaLaCadena() 
    {
        if (yaSeRompio) return;
        yaSeRompio = true;

        // Buscamos todos los eslabones hijos y les activamos la física
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) 
        {
            rb.isKinematic = false; // Caen al suelo
            rb.transform.SetParent(null); // Se independizan para no moverse con el enemigo
            // Opcional: añadir una pequeña fuerza para que salten
            rb.AddExplosionForce(200f, transform.position, 1f); 
        }

        // Avisamos al jefe que una cadena menos
        controlador.CadenaRompida();
    }
}