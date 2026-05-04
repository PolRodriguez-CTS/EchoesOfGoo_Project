using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int _coinValue = 1;
    private float _spawnTime;

    void Start()
    {
        //_spawnTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (Time.time < _spawnTime + 0.2f) return;

        // Esto te dirá en la Consola quién tocó la moneda
        Debug.Log("La moneda fue tocada por: " + other.name + " con el Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            RecogerMoneda();
        }
    }

    void RecogerMoneda()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(_coinValue);
        }
        SoundManager.PlaySound(SoundType.Attack1);
        Destroy(gameObject);
    }
}