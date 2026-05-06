using UnityEngine;

public class ControladorPrisionero : MonoBehaviour 
{
    public int cadenasRestantes = 3;
    public Animator anim;

    public void CadenaRompida() 
    {
        cadenasRestantes--;

        if (cadenasRestantes <= 0) 
        {
            LiberarEnemigo();
        }
    }

    void LiberarEnemigo() 
    {
        // Aquí disparas la animación de levantarse
        anim.SetTrigger("Unchained"); 
        Debug.Log("¡El enemigo es libre!");
    }
}
