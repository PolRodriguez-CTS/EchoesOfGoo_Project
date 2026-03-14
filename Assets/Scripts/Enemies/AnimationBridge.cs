using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private IAtacante scriptPadre;

    void Start()
    {
        // Busca CUALQUIER script en el padre que use la interfaz IAtacante
        scriptPadre = GetComponentInParent<IAtacante>();

        if (scriptPadre == null)
        {
            Debug.LogWarning($"¡Ojo! No encontré ninguna IA con IAtacante en el padre de {gameObject.name}");
        }
    }

    // Esta es la función que pones en el Evento de Animación (FBX)
    public void PlayerDamage()
    {
        if (scriptPadre != null)
        {
            scriptPadre.PlayerDamage(); 
        }
    }
}
