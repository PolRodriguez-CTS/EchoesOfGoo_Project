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

    [Header("Camera")]
    private Transform _mainCamera;
    [SerializeField] private CinemachineCamera _thirdPersonCamera;
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

        Gravity();           // 1. Calcula la caída y timers
        HandleJumpInput();   // 2. Gestiona el salto (suelo y aire)
        HandleDashInput();   // 3. Gestiona el dash
        ApplyMovement();     // 4. Mueve al personaje
        HandleEnergy();      // 5. Recupera energía
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

    void ApplyMovement()
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

        // Si estamos dasheando, la gravedad cae muy lento (estilo Gravity Rush)
        Vector3 gravityToApply = _playerGravity;
        if (_isButtonHeld) gravityToApply.y *= 0.1f;

        _controller.Move(targetDirection * speed * Time.deltaTime + gravityToApply * Time.deltaTime);
        _animator.SetFloat("Speed", speed);
    }

    void HandleEnergy()
    {
        float rate = _isButtonHeld ? -_energyConsumptionRate : _energyRecoveryRate;
        _currentEnergy = Mathf.Clamp(_currentEnergy + rate * Time.deltaTime, 0, _maxDashEnergy);
    }
    #endregion

    #region Jump Logic
    void HandleJumpInput()
    {
        // 1. PRIMER SALTO
        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            if (_jumpTimeOutDelta <= 0) // Solo si el cooldown terminó
            {
                Jump(_jumpHeight);
                _canDoubleJump = true; 
            }
        }
        // 2. INICIAR CARGA DE SEGUNDO SALTO
        else if (_jumpAction.WasPressedThisFrame() && !IsGrounded() && _canDoubleJump)
        {
            _isChargingJump = true;
            _canDoubleJump = false;
            _chargeTimeCounter = 0;
        }

        // 3. PROCESAR CARGA
        if (_isChargingJump)
        {
            if (_jumpAction.IsPressed() && _chargeTimeCounter < _maxChargeTime)
            {
                _chargeTimeCounter += Time.deltaTime;
                // Mantener al jugador "flotando" un poco mientras carga
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

    void Gravity()
    {
        bool grounded = IsGrounded();
        _animator.SetBool("Grounded", grounded);

        if (grounded)
        {
            _fallTimeOutDelta = fallTimeOut;
            _animator.SetBool("Jump", false);
            _animator.SetBool("Fall", false);

            if (_playerGravity.y < 0) _playerGravity.y = -2f;

            // IMPORTANTE: Esto permite que el salto sea usable de nuevo
            if (_jumpTimeOutDelta >= 0) _jumpTimeOutDelta -= Time.deltaTime;
            
            _isChargingJump = false;
            _canDoubleJump = true;
        }
        else
        {
            _jumpTimeOutDelta = jumpTimeOut;

            if (_fallTimeOutDelta >= 0) _fallTimeOutDelta -= Time.deltaTime;
            else _animator.SetBool("Fall", true);

            // Solo aplicar gravedad normal si no estamos cargando el salto
            if (!_isChargingJump)
            {
                _playerGravity.y += _gravity * Time.deltaTime;
            }
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }
    #endregion

    #region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_sensor != null) Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
    #endregion
}