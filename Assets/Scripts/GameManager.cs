using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Main Canvas Reference")]
    [SerializeField] private GameObject _hudCanvas; // Arrastra el objeto "HUD" aquí

    [Header("Scene Settings")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu"; // Nombre exacto de tu escena de menú
    [SerializeField] private string _coreSceneName = "Core"; // <--- Nueva variable
    
    private float _animTimer;
    private int _currentFrame;

    public bool canDash = true;
    public bool canAttack = true;

    [Header("Coin UI")]
    [SerializeField] private Text _coinText;
    private int _totalCoins = 0;

    [Header("UI Settings")]
[SerializeField] private GameObject _turboUIContainer; // Arrastra aquí el objeto de la barra azul



    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }

        // Escondemos las imágenes al empezar
        ToggleTurboVisuals(false);
    }

    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Fabrica")
        {
            canDash = false;
            canAttack = false;
        }
        else
        {
            canDash = true;
            canAttack = true;
        }
    }

    public void AddCoins(int amount)
    {
        _totalCoins += amount;
        UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        if (_coinText != null)
        {
            // Actualizamos el texto legacy
            _coinText.text = _totalCoins.ToString();
        }
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

    private void OnEnable()
    {
        // Nos suscribimos al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Nos desuscribimos para evitar errores de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Este método se ejecuta automáticamente cada vez que cambia la escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_hudCanvas == null) return;

        // Comprobamos si la escena cargada es cualquiera de las excluidas
        bool isNonGameplayScene = scene.name == _mainMenuSceneName || scene.name == _coreSceneName;

        if (isNonGameplayScene)
        {
            _hudCanvas.SetActive(false);
        }
        else
        {
            _hudCanvas.SetActive(true);
        }

        UpdateCoinUI();
    }

    public void SetTurboUIVisibility(bool visible)
{
    if (_turboUIContainer != null && _turboUIContainer.activeSelf != visible)
    {
        _turboUIContainer.SetActive(visible);
    }
}
}