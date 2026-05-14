using UnityEngine;
using System.Collections;
using UnityEngine.Video;

public class VideoIntroHandler : MonoBehaviour
{
    [Header("Referencias")]
    public VideoPlayer videoPlayer;
    public CanvasGroup videoCanvasGroup; // Arrastra aquí el objeto con el Canvas Group
    public MonoBehaviour playerScript;
    public GameObject gameHUD;

    [Header("Ajustes de Fade")]
    public float fadeDuration = 1.5f; // Duración del fundido en segundos

    void Start()
    {
        // Bloqueamos al jugador al empezar
        playerScript.enabled = false;
        if (gameHUD != null) gameHUD.SetActive(false);

        // Opacidad al máximo al inicio
        videoCanvasGroup.alpha = 1f;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // En lugar de desactivar de golpe, lanzamos el fundido
        StartCoroutine(FadeOutAndStartGame());
    }

    IEnumerator FadeOutAndStartGame()
    {
        float currentTime = 0;

        // Mientras no hayamos llegado al tiempo de duración...
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Calculamos la opacidad de 1 a 0
            videoCanvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            yield return null; // Espera al siguiente frame
        }

        // Una vez invisible, activamos todo lo demás
        videoCanvasGroup.alpha = 0f;
        playerScript.enabled = true;
        if (gameHUD != null) gameHUD.SetActive(true);

        // Finalmente desactivamos el objeto para ahorrar recursos
        videoCanvasGroup.gameObject.SetActive(false);
        
        Debug.Log("Fade terminado y juego iniciado.");
    }
}