using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeHandler : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Coroutine currentFade;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Aparecer(float duracion)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        canvasGroup.blocksRaycasts = true; // Permite clicks si los hubiera
        currentFade = StartCoroutine(FadeRoutine(1f, duracion));
    }

    public void Desaparecer(float duracion)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        canvasGroup.blocksRaycasts = false; // Bloquea clicks al desaparecer
        currentFade = StartCoroutine(FadeRoutine(0f, duracion));
    }

    private IEnumerator FadeRoutine(float alphaObjetivo, float duracion)
    {
        float alphaInicial = canvasGroup.alpha;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(alphaInicial, alphaObjetivo, tiempo / duracion);
            yield return null;
        }

        canvasGroup.alpha = alphaObjetivo;

        // Si hemos desaparecido del todo, apagamos el objeto para ahorrar rendimiento
        if (alphaObjetivo == 0f)
        {
            gameObject.SetActive(false);
        }
    }
}