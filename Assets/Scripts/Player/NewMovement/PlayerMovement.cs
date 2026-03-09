using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    private CharacterController _controller;
    private Animator _animator;
    
    private InputAction _moveAction, _jumpAction, _dashAction, _lookAction, _aimAction, _grapplingAction;
    
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

    [Header("Jump & Gravity")]
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private Vector3 _playerGravity;
    [SerializeField] private float _gravity = -15f;
    public float jumpTimeOut = 0.05f;
    public float fallTimeOut = 0.15f;
    float _jumpTimeOutDelta;
    float _fallTimeOutDelta;

    [Header("Sustained Boost (Dash)")]
    [SerializeField] private float _maxDashEnergy = 100;
    [SerializeField] private float _energyConsumptionRate = 40f;
    [SerializeField] private float _energyRecoveryRate = 20f;
    private float _currentEnergy;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _topSpeed = 25f;
    private bool _isButtonHeld = false;

    [Header("Grappling")]
    [SerializeField] private float _grappleSpeed = 40f;
    [SerializeField] private float _maxGrappleDistance = 50f;
    [SerializeField] private LayerMask _grappableLayer; 
    [SerializeField] private LineRenderer _lineRenderer; 
    private Vector3 _externalMomentum;
    private Vector3 _grapplePoint;
    private bool _isGrappling = false;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera _thirdPersonCamera;
    private Transform _mainCamera;
    private bool isCameraBlending; // Mantener por lógica de Gravity

    #endregion

    #region Main Methods
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _dashAction = InputSystem.actions["Sprint"];
        _lookAction = InputSystem.actions["Look"];
        _aimAction = InputSystem.actions["Aim"];
        _grapplingAction = InputSystem.actions["Grapple"];

        _mainCamera = Camera.main.transform;
        _thirdPersonCamera.Prioritize();
        
        _currentEnergy = _maxDashEnergy;
    }

    void Start()
    {
        _jumpTimeOutDelta = jumpTimeOut;
        _fallTimeOutDelta = fallTimeOut;
    }

    void Update()
    {
        if (isCameraBlending)
        {
            Gravity();
            return; 
        }

        _moveInput = _moveAction.ReadValue<Vector2>();
        _animator.SetFloat("Horizontal", _moveInput.x);
        _animator.SetFloat("Vertical", _moveInput.y);

        Gravity();
        HandleGrappleInput(); // Detección de gancho
        HandleDashInput();    // Detección de dash
        ApplyMovement();      // Ejecución de movimiento unificado
        HandleEnergy();       // Gestión de estamina
        
        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }
    }
    #endregion

    #region Grappling Logic
    void HandleGrappleInput()
    {
        if (_grapplingAction.WasPressedThisFrame())
        {
            StartGrapple();
        }
        
        if (_grapplingAction.WasReleasedThisFrame())
        {
            StopGrapple();
        }

        if (_isGrappling)
        {
            _lineRenderer.SetPosition(0, transform.position); // Actualiza origen del cable
            ExecuteGrapple(_grapplePoint); // Aplica el tirón constante

            if (Vector3.Distance(transform.position, _grapplePoint) < 2f)
            {
                StopGrapple();
            }
        }
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _maxGrappleDistance, _grappableLayer))
        {
            _grapplePoint = hit.point;
            _isGrappling = true;
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(1, _grapplePoint);
        }
    }

    void StopGrapple()
    {
        _isGrappling = false;
        _lineRenderer.enabled = false;
    }

    public void ExecuteGrapple(Vector3 targetPoint)
    {
        Vector3 grappleDir = (targetPoint - transform.position).normalized;
        _externalMomentum = grappleDir * _grappleSpeed;
        
        // Despegar un poco del suelo para evitar fricción
        if(IsGrounded()) _playerGravity.y = 2f; 
    }
    #endregion

    #region Movement & Dash Logic
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
        Vector3 inputDir = new Vector3(_moveInput.x, 0, _moveInput.y);
        Vector3 targetDirection = transform.forward;

        if (inputDir.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            targetDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
        }

        float targetSpeed = _isButtonHeld ? _topSpeed : _movementSpeed;
        if (inputDir.magnitude == 0 && !_isButtonHeld) targetSpeed = 0;

        float accelRate = _isButtonHeld ? _acceleration : _speedChangeRate;
        speed = Mathf.MoveTowards(speed, targetSpeed, accelRate * Time.deltaTime);

        // Inercia del momentum (Grappling/Dash previo)
        if (_externalMomentum.magnitude > 0.1f)
        {
            _externalMomentum = Vector3.Lerp(_externalMomentum, Vector3.zero, Time.deltaTime * 3f);
        }
        else
        {
            _externalMomentum = Vector3.zero;
        }

        // Mover final
        Vector3 moveVector = (targetDirection * speed) + _externalMomentum + _playerGravity;
        _controller.Move(moveVector * Time.deltaTime);
        
        _animator.SetFloat("Speed", speed);
    }

    void HandleEnergy()
    {
        if (_isButtonHeld)
        {
            _currentEnergy -= _energyConsumptionRate * Time.deltaTime;
        }
        else
        {
            _currentEnergy += _energyRecoveryRate * Time.deltaTime;
        }
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, _maxDashEnergy);
    }
    #endregion

    #region Physics & Gravity
    void Jump()
    {
        if(_jumpTimeOutDelta <= 0)
        {
            _animator.SetBool("Jump", true);
            _playerGravity.y = Mathf.Sqrt(_jumpHeight * -2 * _gravity);
        }
    }

    void Gravity()
    {
        bool grounded = IsGrounded();
        _animator.SetBool("Grounded", grounded);

        if(grounded)
        {
            _fallTimeOutDelta = fallTimeOut;
            _animator.SetBool("Jump", false);
            _animator.SetBool("Fall", false);

            if(_playerGravity.y < 0) _playerGravity.y = -2;

            if(_jumpTimeOutDelta >= 0) _jumpTimeOutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeOutDelta = jumpTimeOut;
            if(_fallTimeOutDelta >= 0) _fallTimeOutDelta -= Time.deltaTime;
            else _animator.SetBool("Fall", true);

            _playerGravity.y += _gravity * Time.deltaTime;
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }
    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
}
