using UnityEngine;
public class Eslabon : MonoBehaviour 
{
    public Cadena miCadenaPadre;

    public void RecibirGolpe() 
    {
        miCadenaPadre.RomperTodaLaCadena();
    }
}