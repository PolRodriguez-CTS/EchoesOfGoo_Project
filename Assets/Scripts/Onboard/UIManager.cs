using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Ya no necesitamos "panelControles", ahora es dinámico
    private Coroutine controlesCoroutine;
    private GameObject panelActual; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Esta función ahora pide QUÉ panel encender y CUÁNTO tiempo
    public void MostrarControlMecanica(GameObject panelMecanica, float tiempo)
    {
        // Si ya había un tutorial de control en pantalla, lo apaga primero
        if (controlesCoroutine != null) 
        {
            StopCoroutine(controlesCoroutine);
            if (panelActual != null) panelActual.SetActive(false);
        }

        controlesCoroutine = StartCoroutine(ControlMecanicaCoroutine(panelMecanica, tiempo));
    }

    private IEnumerator ControlMecanicaCoroutine(GameObject panelMecanica, float tiempo)
    {
        panelActual = panelMecanica;
        panelActual.SetActive(true);

        yield return new WaitForSeconds(tiempo);

        panelActual.SetActive(false);
        panelActual = null;
    }
}