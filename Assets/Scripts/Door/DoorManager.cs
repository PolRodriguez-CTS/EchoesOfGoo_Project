using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public int platesRequired = 2;
    private int platesActive = 0;
    public Door doorScript; // Arrastra aquí el objeto con el script Door

    public void PlateActivated()
    {
        platesActive++;
        if (platesActive >= platesRequired)
        {
            doorScript.OpenAnimation();
        }
    }

    /*
    public void PlateDeactivated()
    {
        platesActive--;
        // Opcional: Cerrar si una placa se libera
        if (platesActive < platesRequired)
        {
            doorScript.CloseAnimation();
        }
    }
    */
}