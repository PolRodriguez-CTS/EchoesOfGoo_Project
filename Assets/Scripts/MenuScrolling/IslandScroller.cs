using UnityEngine;

public class IslandScroller : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    public float speed = 5f;          // Velocidad de la isla
    public float leftLimit = -20f;    // Punto donde desaparece (X izquierda)
    public float rightLimit = 20f;   // Punto donde reaparece (X derecha)

    [Header("Variación (Opcional)")]
    public float minY = -3f;          // Altura mínima aleatoria
    public float maxY = 3f;           // Altura máxima aleatoria

    void Update()
    {
        // 1. Mover la isla hacia la izquierda
        transform.Translate(Vector2.left * speed * Time.deltaTime, Space.World);

        // 2. Comprobar si salió de la pantalla
        if (transform.position.x <= leftLimit)
        {
            RepositionIsland();
        }
    }

    void RepositionIsland()
    {
        // 3. Teletransportar a la derecha con una altura Y aleatoria para variedad
        float randomY = Random.Range(minY, maxY);
        transform.position = new Vector3(rightLimit, randomY, transform.position.z);
    }
}