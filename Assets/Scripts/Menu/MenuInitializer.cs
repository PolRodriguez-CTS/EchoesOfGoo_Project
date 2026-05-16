using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class MenuInitializer : MonoBehaviour
{
    [SerializeField] private Button firstSelectedButton;

    void Start()
    {
        // 1. Buscamos el EventSystem que esté vivo en la escena (ya sea de Core o de esta)
        EventSystem currentEventSystem = EventSystem.current;

        if (currentEventSystem != null && firstSelectedButton != null)
        {
            // 2. Limpiamos cualquier selección previa (por si acaso)
            currentEventSystem.SetSelectedGameObject(null);
            
            // 3. Le asignamos a la fuerza el botón del menú
            currentEventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
            
            Debug.Log($"Mando redirigido con éxito a: {firstSelectedButton.name}");
        }
        else
        {
            Debug.LogWarning("¡Ojo! No se encontró un EventSystem activo o no has arrastrado el botón en el inspector.");
        }
    }
}
