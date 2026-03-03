using UnityEngine;
using System.Collections;
public class Blink : MonoBehaviour
{
    [Header("Configuración del BlendShape")]
    public SkinnedMeshRenderer characterMesh;
    public int blinkShapeIndex = 1; // El índice del blend shape de "pestañear"

    [Header("Tiempos del Parpadeo")]
    public float closeDuration = 0.05f;  // El cierre es casi instantáneo
    public float openDuration = 0.15f;   // La apertura es un poco más pausada
    public float minWaitTime = 2.0f;     // Tiempo mínimo entre parpadeos
    public float maxWaitTime = 5.0f;     // Tiempo máximo entre parpadeos

    void Start()
    {
        if (characterMesh != null)
        {
            StartCoroutine(BlinkRoutine());
        }
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Espera un tiempo aleatorio para que no sea rítmico (orgánico)
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            // Fase 1: Cerrar el ojo (Rápido)
            float timer = 0;
            while (timer < closeDuration)
            {
                timer += Time.deltaTime;
                float weight = Mathf.Lerp(0, 100, timer / closeDuration);
                characterMesh.SetBlendShapeWeight(blinkShapeIndex, weight);
                yield return null;
            }

            // Fase 2: Abrir el ojo (Un poco más lento)
            timer = 0;
            while (timer < openDuration)
            {
                timer += Time.deltaTime;
                // Usamos una curva de desaceleración simple (SmoothStep)
                float progress = timer / openDuration;
                float weight = Mathf.Lerp(100, 0, Mathf.Sin(progress * Mathf.PI * 0.5f)); 
                characterMesh.SetBlendShapeWeight(blinkShapeIndex, weight);
                yield return null;
            }

            // Asegurar que quede totalmente abierto al final
            characterMesh.SetBlendShapeWeight(blinkShapeIndex, 0);
        }
    }
}
