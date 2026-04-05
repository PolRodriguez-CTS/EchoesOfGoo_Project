using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    //Components
    private CharacterController _controller;
    private Animator _animator;
    
    //Inputs
    private InputAction _moveAction, _jumpAction, _dashAction, _lookAction, _aimAction;
    
    [Header("Movement")]
    private Vector2 _moveInput;
    [SerializeField] private float _movementSpeed = 7.5f;
    [SerializeField] private float _smoothTime = 0.1f;
    private float _turnSmoothVelocity;
    public float _speedChangeRate = 50;
    private float speed;
    private float _mainCameraEulerY;

    [Header("Ground Sensor")]
    [SerializeField] private Transform _sensor;
    [SerializeField] float _sensorRadius = 0.5f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Double Jump Loaded")]
    [SerializeField] private float _maxChargeTime = 0.6f;     
    [SerializeField] private float _extraJumpForce = 1f;    
    [SerializeField] private float _chargeGravityScale = 0.15f; 
    private float _chargeTimeCounter;
    private bool _canDoubleJump = false;
    private bool _isChargingJump = false;

    [Header("Jump & Gravity")]
    [SerializeField] private float _jumpHeight = 2.5f;
    [SerializeField] private Vector3 _playerGravity;
    [SerializeField] private float _gravity = -15f;
    public float jumpTimeOut = 0.1f;
    public float fallTimeOut = 0.15f;
    private float _jumpTimeOutDelta;
    private float _fallTimeOutDelta;

    [Header("Sustained Boost")]
    [SerializeField] private float _maxDashEnergy = 100;
    [SerializeField] private float _energyConsumptionRate = 40f;
    [SerializeField] private float _energyRecoveryRate = 20f;
    private float _currentEnergy;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _topSpeed = 25f;
    private bool _isButtonHeld = false;

    [Header("External Boost")]
    private Vector3 _externalImpulse;
    private float _impulseTimeDelta;

    [Header("External Impulse Settings")]
    [SerializeField] private float _impulseDeceleration = 5f; // Cuánto "aire" frena el impulso (más alto = frena antes)
    private bool _isImpulseActive = false; // Para saber si estamos bajo el efecto de un turbo

    [Header("Camera")]
    private Transform _mainCamera;
    [SerializeField] private CinemachineCamera _thirdPersonCamera;
    [SerializeField] private float _baseFOV = 60f;
    [SerializeField] private float _dashFOV = 90f;
    [SerializeField] private float _fovSmoothSpeed = 10f;
    #endregion

    #region Awake & Start
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        var actions = InputSystem.actions;
        _moveAction = actions["Move"];
        _jumpAction = actions["Jump"];
        _dashAction = actions["Sprint"];
        _lookAction = actions["Look"];

        _mainCamera = Camera.main.transform;
        _currentEnergy = _maxDashEnergy;
    }

    void Start()
    {
        _jumpTimeOutDelta = jumpTimeOut;
        _fallTimeOutDelta = fallTimeOut;
    }
    #endregion

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _animator.SetFloat("Horizontal", _moveInput.x);
        _animator.SetFloat("Vertical", _moveInput.y);

        Gravity2();           // 1. Calcula la caída y timers
        HandleJumpInput4();   // 2. Gestiona el salto (suelo y aire)
        HandleDashInput();   // 3. Gestiona el dash
        ApplyMovement2();     // 4. Mueve al personaje
        HandleEnergy();      // 5. Recupera energía

        HandleSpeedEffects();
    }

    #region Movement & Dash
    void HandleDashInput()
    {
        if (_dashAction.WasPressedThisFrame() && _currentEnergy > 10f)
        {
            _isButtonHeld = true;
            _animator.SetBool("isDashing", true);
        }

        if (_dashAction.WasReleasedThisFrame() || _currentEnergy <= 0)
        {
            _isButtonHeld = false;
            _animator.SetBool("isDashing", false);
        }
    }

    void HandleSpeedEffects()
    {
        // 1. EFECTO DE CÁMARA (FOV)
        // Cinemachine usa 'Lens.FieldOfView' en la nueva versión
        float targetFOV = (_isButtonHeld || _isImpulseActive) ? _dashFOV : _baseFOV;
        _thirdPersonCamera.Lens.FieldOfView = Mathf.Lerp(_thirdPersonCamera.Lens.FieldOfView, targetFOV, Time.deltaTime * _fovSmoothSpeed);
    }

    void ApplyMovement2()
{
    Vector3 inputDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
    Vector3 targetDirection = transform.forward;

    if (inputDir.magnitude > 0.1f)
    {
        float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
        transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
        targetDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
    }

    float targetSpeed = _isButtonHeld ? _topSpeed : (inputDir.magnitude > 0 ? _movementSpeed : 0);
    float accelRate = _isButtonHeld ? _acceleration : _speedChangeRate;
    speed = Mathf.MoveTowards(speed, targetSpeed, accelRate * Time.deltaTime);

    // --- LÓGICA DE IMPULSO EXTERNO SUAVE ---
    _externalImpulse = Vector3.Lerp(_externalImpulse, Vector3.zero, _impulseDeceleration * Time.deltaTime);

    if (_externalImpulse.sqrMagnitude < 0.1f) 
    {
        _externalImpulse = Vector3.zero;
        _isImpulseActive = false; 
    }

    // --- MOVIMIENTO FINAL ---
    // 1. Calculamos el movimiento horizontal (Caminado/Dash + Impulso Externo)
    Vector3 horizontalMovement = (targetDirection * speed) + _externalImpulse;

    // 2. Aplicamos el movimiento total sumando la gravedad (que ya se calculó en Gravity2)
    // USAMOS (movimiento horizontal + gravedad vertical) * Time.deltaTime
    _controller.Move((horizontalMovement + _playerGravity) * Time.deltaTime);
    
    _animator.SetFloat("Speed", speed);
}

    void HandleEnergy()
    {
        float rate = _isButtonHeld ? -_energyConsumptionRate : _energyRecoveryRate;
        _currentEnergy = Mathf.Clamp(_currentEnergy + rate * Time.deltaTime, 0, _maxDashEnergy);
    }
    #endregion

    #region Jump Logic

    void HandleJumpInput4()
    {
        // 1. INTENTO DE SALTO (Al presionar el botón)
        if (_jumpAction.WasPressedThisFrame())
        {
            // CASO A: Estoy en el suelo (Salto normal)
            if (IsGrounded())
            {
                if (_jumpTimeOutDelta <= 0)
                {
                    Jump(_jumpHeight);
                    _canDoubleJump = true; // Habilitamos el segundo para el aire
                }
            }
            // CASO B: Estoy en el aire y tengo el doble salto disponible
            else if (_canDoubleJump)
            {
                // Iniciamos la carga del segundo salto
                _isChargingJump = true;
                _canDoubleJump = false; // CRUCIAL: Bloqueamos cualquier otro salto extra inmediatamente
                _chargeTimeCounter = 0;
            }
        }

        // 2. PROCESAR LA CARGA (Mientras se mantiene presionado)
        if (_isChargingJump)
        {
            // Si el jugador suelta el botón o llega al tiempo máximo
            if (_jumpAction.IsPressed() && _chargeTimeCounter < _maxChargeTime)
            {
                _chargeTimeCounter += Time.deltaTime;
                // Flotación durante carga
                _playerGravity.y = Mathf.Lerp(_playerGravity.y, _gravity * _chargeGravityScale, Time.deltaTime * 10f);
            }
            else
            {
                ReleaseChargedJump();
            }
        }
    }

    void Jump(float height)
    {
        _animator.SetBool("Jump", true);
        _animator.SetBool("Fall", false);
        _playerGravity.y = Mathf.Sqrt(height * -2f * _gravity);
    }

    void ReleaseChargedJump()
    {
        if (!_isChargingJump) return;
        _isChargingJump = false;

        float chargePercent = _chargeTimeCounter / _maxChargeTime;
        float finalJumpHeight = _jumpHeight + (_extraJumpForce * chargePercent);
        
        _playerGravity.y = Mathf.Sqrt(finalJumpHeight * -2f * _gravity);
        _animator.SetTrigger("DoubleJump"); // Asegúrate de tener este Trigger en el Animator
    }

    void Gravity2()
    {
        bool grounded = IsGrounded();
        _animator.SetBool("Grounded", grounded);

        if (grounded)
        {
            _fallTimeOutDelta = fallTimeOut;
            _animator.SetBool("Jump", false);
            _animator.SetBool("Fall", false);

            // Solo reseteamos la gravedad si NO acabamos de saltar
            if (_playerGravity.y < 0) _playerGravity.y = -2f;

            if (_jumpTimeOutDelta >= 0) _jumpTimeOutDelta -= Time.deltaTime;
            
            _isChargingJump = false;
            _canDoubleJump = true;
        }
        else
        {
            _jumpTimeOutDelta = jumpTimeOut;
            if (_fallTimeOutDelta >= 0) _fallTimeOutDelta -= Time.deltaTime;
            else _animator.SetBool("Fall", true);

            if (!_isChargingJump)
            {
                // 1. Calculamos cuánto pesa el personaje en este frame
                float gravityMultiplier = 1.0f;

                // Si estamos haciendo DASH y ADEMÁS ya estamos cayendo...
                if (_isButtonHeld && _playerGravity.y < 0) 
                {
                    gravityMultiplier = 0.1f; // Caída lenta estilo "Gravity Rush"
                }
                else 
                {
                    gravityMultiplier = 1.0f; // Salto normal y firme
                }

                // 2. Aplicamos la gravedad con el multiplicador inteligente
                _playerGravity.y += _gravity * gravityMultiplier * Time.deltaTime;

                // 3. Limitador de velocidad (Terminal Velocity)
                // Ajusta el -20f a tu gusto: más bajo = cae más rápido
                if (_playerGravity.y < -20f) 
                {
                    _playerGravity.y = -20f;
                }
            }
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }
    #endregion
/*
    public void ApplyExternalImpulse(Vector3 force, float duration)
    {
        _externalImpulse = force;
        _impulseTimeDelta = duration;
    }
*/

public void ApplyExternalImpulse(Vector3 force)
{
    // Sumamos la fuerza (esto permite acumular turbos si entras en varios seguidos)
    _externalImpulse += force;
    _isImpulseActive = true; // Activamos el efecto de cámara (FOV)
}

    public void ApplyBounce(float bounceHeight, bool resetDoubleJump)
    {
        // 1. EL TRUCO: Reseteamos la gravedad acumulada inmediatamente.
        // Al ponerlo a 0, eliminamos toda la "pesadez" de la caída previa.
        _playerGravity.y = 0f;

        // 2. Aplicamos la nueva fuerza de salto vertical.
        // Usamos la misma fórmula de tu método Jump(): sqrt(h * -2 * g)
        _playerGravity.y = Mathf.Sqrt(bounceHeight * -2f * _gravity);

        // 3. Gestión de animaciones y estados
        _animator.SetBool("Jump", true);
        _animator.SetBool("Fall", false);
        
        // Cancelamos cualquier carga de salto que estuviera en proceso
        _isChargingJump = false; 

        // 4. Opcional: Resetear el doble salto
        if (resetDoubleJump)
        {
            _canDoubleJump = true;
        }
    }

    #region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_sensor != null) Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
    #endregion
}