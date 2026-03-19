using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    private CharacterController _controller;
    private Animator _animator;
    private InputAction _moveAction, _jumpAction, _dashAction, _lookAction;
    
    [Header("Movement")]
    private Vector2 _moveInput;
    [SerializeField] private float _movementSpeed = 7.5f;
    [SerializeField] private float _smoothTime = 0.1f;
    private float _turnSmoothVelocity;
    public float _speedChangeRate = 50;
    private float speed;

    [Header("Ground Sensor")]
    [SerializeField] private Transform _sensor;
    [SerializeField] float _sensorRadius = 0.5f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Double Jump Loaded")]
    [SerializeField] private float _maxChargeTime = 0.6f;
    [SerializeField] private float _extraJumpForce = 8f;
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

    [Header("Sustained Boost (Dash)")]
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

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _dashAction = InputSystem.actions["Sprint"];
        _lookAction = InputSystem.actions["Look"];
        _mainCamera = Camera.main.transform;
        _currentEnergy = _maxDashEnergy;
    }

    void Start()
    {
        _jumpTimeOutDelta = jumpTimeOut;
        _fallTimeOutDelta = fallTimeOut;
    }

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _animator.SetFloat("Horizontal", _moveInput.x);
        _animator.SetFloat("Vertical", _moveInput.y);

        Gravity();
        HandleJumpInput();
        HandleDashInput();
        ApplyMovement();
        HandleEnergy();
    }

    void HandleJumpInput()
    {
        // 1. PRIMER SALTO
        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            if (_jumpTimeOutDelta <= 0) 
            {
                Jump(_jumpHeight);
                // CORRECCIÓN 1: Forzamos a que el doble salto sea posible tras el primer salto
                _canDoubleJump = true; 
            }
        }
        // 2. SEGUNDO SALTO (Solo si no estamos en el suelo y el flag es true)
        else if (_jumpAction.WasPressedThisFrame() && !IsGrounded() && _canDoubleJump && !_isChargingJump)
        {
            _isChargingJump = true;
            _canDoubleJump = false; 
            _chargeTimeCounter = 0;
        }

        if (_isChargingJump)
        {
            if (_jumpAction.IsPressed() && _chargeTimeCounter < _maxChargeTime)
            {
                _chargeTimeCounter += Time.deltaTime;
                // CORRECCIÓN 2: Suavizamos la caída durante la carga
                _playerGravity.y = Mathf.Lerp(_playerGravity.y, _gravity * _chargeGravityScale, Time.deltaTime * 5f);
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
        _animator.SetTrigger("DoubleJump"); 
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

            if (_jumpTimeOutDelta >= 0) _jumpTimeOutDelta -= Time.deltaTime;
            
            _isChargingJump = false;
            // CORRECCIÓN 3: NO pongas _canDoubleJump aquí en false, déjalo que HandleJumpInput lo controle.
        }
        else
        {
            _jumpTimeOutDelta = jumpTimeOut;
            if (_fallTimeOutDelta >= 0) _fallTimeOutDelta -= Time.deltaTime;
            else _animator.SetBool("Fall", true);

            if (!_isChargingJump)
            {
                _playerGravity.y += _gravity * Time.deltaTime;
            }
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

        Vector3 gravityToMove = _playerGravity;
        if (_isButtonHeld) gravityToMove.y *= 0.1f;

        _controller.Move(targetDirection * speed * Time.deltaTime + gravityToMove * Time.deltaTime);
        _animator.SetFloat("Speed", speed);
    }

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

    void HandleEnergy()
    {
        float rate = _isButtonHeld ? -_energyConsumptionRate : _energyRecoveryRate;
        _currentEnergy = Mathf.Clamp(_currentEnergy + rate * Time.deltaTime, 0, _maxDashEnergy);
    }

    bool IsGrounded()
    {
        // Asegúrate de que el sensor NO esté atravesando el suelo en el editor
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_sensor != null) Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
}