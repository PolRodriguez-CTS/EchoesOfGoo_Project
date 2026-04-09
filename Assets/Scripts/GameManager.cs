using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Energy")]
    [SerializeField] private Image _energyBarImage;

    [Header("Turbo Animations")]
    [SerializeField] private GameObject[] _turboVisuals; // Arrastra tus 3 imágenes aquí
    [SerializeField] private float _animationSpeed = 0.1f; // Velocidad del parpadeo

    [Header("Health UI")]
    [SerializeField] private GameObject[] _healthSlots;
    
    private float _animTimer;
    private int _currentFrame;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }

        // Escondemos las imágenes al empezar
        ToggleTurboVisuals(false);
    }

    public void UpdateHealthUI(float currentHealth)
    {
        for (int i = 0; i < _healthSlots.Length; i++)
        {
            // Si el índice es menor que la vida actual, el slot se activa
            // Ejemplo: Si vida es 2, el slot 0 y 1 se activan, el 2 y 3 se apagan
            if (i < currentHealth)
            {
                _healthSlots[i].SetActive(true);
            }
            else
            {
                _healthSlots[i].SetActive(false);
            }
        }
    }

    public void UpdateTurboUI(float currentEnergy, float maxEnergy, bool isDashing)
    {
        // 1. Actualizar la barra (lo que ya teníamos)
        if (_energyBarImage != null)
            _energyBarImage.fillAmount = currentEnergy / maxEnergy;

        // 2. Controlar la animación de las 3 imágenes
        if (isDashing && currentEnergy > 0)
        {
            AnimateTurbo();
        }
        else
        {
            ToggleTurboVisuals(false);
        }
    }

    private void AnimateTurbo()
    {
        _animTimer += Time.deltaTime;

        if (_animTimer >= _animationSpeed)
        {
            _animTimer = 0;
            
            // Apagamos todas
            ToggleTurboVisuals(false);

            // Encendemos la siguiente en el ciclo
            _currentFrame = (_currentFrame + 1) % _turboVisuals.Length;
            _turboVisuals[_currentFrame].SetActive(true);
        }
    }

    private void ToggleTurboVisuals(bool state)
    {
        foreach (var img in _turboVisuals)
        {
            if (img != null) img.SetActive(state);
        }
    }
}