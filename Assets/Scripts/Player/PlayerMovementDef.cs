using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementDef : MonoBehaviour
{
    // Componentes
    private CharacterController _controller;
    private Animator _animator;
    private Transform _mainCamera;

    // Inputs
    private InputAction _moveAction, _jumpAction, _dashAction;

    [Header("Movement & Rotation")]
    [SerializeField] private float _movementSpeed = 7.5f;
    [SerializeField] private float _topSpeed = 20f;
    [SerializeField] private float _acceleration = 30f;
    [SerializeField] private float _speedChangeRate = 15f;
    [SerializeField] private float _smoothTime = 0.1f;
    private float _speed;
    private float _turnSmoothVelocity;
    private Vector2 _moveInput;

    [Header("Stamina System")]
    [SerializeField] private float _maxEnergy = 100f;
    [SerializeField] private float _energyConsumptionRate = 35f;
    [SerializeField] private float _energyRecoveryRate = 20f;
    private float _currentEnergy;

    [Header("Jump & Gravity")]
    [SerializeField] private float _jumpHeight = 2.5f;
    [SerializeField] private float _gravity = -15f;
    [SerializeField] private float _slowFallGravity = -1.5f;
    private Vector3 _playerGravity;

    [Header("Cargable Double Jump")]
    [SerializeField] private float _baseDoubleJumpForce = 4f;
    [SerializeField] private float _maxDoubleJumpForce = 12f;
    [SerializeField] private float _jumpChargeSpeed = 8f;
    private bool _canDoubleJump;
    private bool _isChargingJump;
    private bool _hasDoubleJumped;
    private float _currentJumpCharge;

    [Header("Dash State")]
    private bool _isDashButtonHeld;

    [Header("Ground Sensor")]
    [SerializeField] private Transform _sensor;
    [SerializeField] float _sensorRadius = 0.4f;
    [SerializeField] private LayerMask _groundLayer;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _mainCamera = Camera.main.transform;

        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _dashAction = InputSystem.actions["Sprint"]; // O la acción que tengas para Dash

        _currentEnergy = _maxEnergy;
    }

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();

        HandleInput();
        HandleEnergy();
        ApplyGravity();
        ApplyMovement();
    }

    private void HandleInput()
    {
        bool grounded = IsGrounded();

        // --- LÓGICA DE DASH ---
        if (_dashAction.WasPressedThisFrame() && _currentEnergy > 10f)
            _isDashButtonHeld = true;
        
        if (_dashAction.WasReleasedThisFrame() || _currentEnergy <= 0)
            _isDashButtonHeld = false;

        // --- LÓGICA DE SALTO ---
        if (_jumpAction.WasPressedThisFrame() && grounded)
        {
            _playerGravity.y = Mathf.Sqrt(_jumpHeight * -2 * _gravity);
            _animator.SetTrigger("Jump");
            _canDoubleJump = true;
            _hasDoubleJumped = false;
        }

        // Iniciar carga de doble salto en el aire
        if (_jumpAction.IsPressed() && !grounded && _canDoubleJump && !_hasDoubleJumped && _currentEnergy > 5f)
        {
            _isChargingJump = true;
        }

        // Ejecutar salto al soltar o agotar energía/carga
        if ((_jumpAction.WasReleasedThisFrame() || _currentEnergy <= 0) && _isChargingJump)
        {
            ExecuteDoubleJump();
        }

        if (grounded)
        {
            _canDoubleJump = false;
            _hasDoubleJumped = false;
            _isChargingJump = false;
            _currentJumpCharge = 0;
        }
    }

    private void ExecuteDoubleJump()
    {
        _isChargingJump = false;
        _hasDoubleJumped = true;
        _canDoubleJump = false;

        // El salto mínimo es _baseDoubleJumpForce, el máximo depende de la carga
        float finalJumpForce = _baseDoubleJumpForce + _currentJumpCharge;
        _playerGravity.y = Mathf.Sqrt(finalJumpForce * -2 * _gravity);

        _animator.SetTrigger("DoubleJump");
        _currentJumpCharge = 0;
    }

    private void HandleEnergy()
    {
        if (_isDashButtonHeld || _isChargingJump)
        {
            _currentEnergy -= _energyConsumptionRate * Time.deltaTime;
            
            // Si estamos cargando salto, acumulamos la fuerza aquí
            if (_isChargingJump)
            {
                _currentJumpCharge += _jumpChargeSpeed * Time.deltaTime;
                _currentJumpCharge = Mathf.Clamp(_currentJumpCharge, 0, _maxDoubleJumpForce);
            }
        }
        else
        {
            _currentEnergy += _energyRecoveryRate * Time.deltaTime;
        }

        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, _maxEnergy);
    }

    private void ApplyMovement()
    {
        // 1. ROTACIÓN: Siempre mira hacia donde mira la cámara
        float cameraYaw = _mainCamera.eulerAngles.y;
        float smoothRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, cameraYaw, ref _turnSmoothVelocity, _smoothTime);
        transform.rotation = Quaternion.Euler(0, smoothRotation, 0);

        // 2. DIRECCIÓN: Strafe relativo a la cámara
        Vector3 inputDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
        Vector3 camForward = Vector3.ProjectOnPlane(_mainCamera.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_mainCamera.right, Vector3.up).normalized;
        Vector3 targetDirection = (camForward * inputDir.z + camRight * inputDir.x);

        // Si no hay input pero hay dash, propulsión hacia adelante
        if (targetDirection.magnitude < 0.1f && _isDashButtonHeld)
            targetDirection = transform.forward;

        // 3. VELOCIDAD: Aceleración y frenado suave
        float targetSpeed = _isDashButtonHeld ? _topSpeed : _movementSpeed;
        if (inputDir.magnitude == 0 && !_isDashButtonHeld) targetSpeed = 0;

        float currentAccel = _isDashButtonHeld ? _acceleration : _speedChangeRate;
        _speed = Mathf.MoveTowards(_speed, targetSpeed, currentAccel * Time.deltaTime);

        // 4. APLICAR MOVIMIENTO FINAL
        Vector3 finalVelocity = (targetDirection * _speed) + _playerGravity;
        _controller.Move(finalVelocity * Time.deltaTime);

        // 5. ANIMACIONES
        _animator.SetFloat("Horizontal", _moveInput.x * (_speed / _movementSpeed));
        _animator.SetFloat("Vertical", _moveInput.y * (_speed / _movementSpeed));
        _animator.SetFloat("Speed", _speed);
        _animator.SetBool("IsDashing", _isDashButtonHeld);
        _animator.SetBool("IsChargingJump", _isChargingJump);
    }

    private void ApplyGravity()
    {
        if (IsGrounded())
        {
            if (_playerGravity.y < 0) _playerGravity.y = -2f;
        }
        else
        {
            // Caída lenta si estamos cargando el salto o en dash sostenido
            float currentGravity = (_isChargingJump || (_isDashButtonHeld && _speed > _movementSpeed)) 
                                    ? _slowFallGravity 
                                    : _gravity;
            
            _playerGravity.y += currentGravity * Time.deltaTime;
        }
    }

    public bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }

    // Útil para ver el sensor de suelo en el editor
    private void OnDrawGizmosSelected()
    {
        if (_sensor != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
        }
    }
}