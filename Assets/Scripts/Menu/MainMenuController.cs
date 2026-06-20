using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    // Este método lo llamará el botón de Salir
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

        // Si estamos ejecutando el juego desde el editor de Unity
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Si es el juego ya exportado (Build final)
            Application.Quit();
        #endif
    }
}
