using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    //Components
    private CharacterController _controller;
    private Animator _animator;
    
    //Inputs
    private InputAction _moveAction, _jumpAction, _dashAction, _lookAction, _aimAction;
    
//----------------------------------------------------------------------------------------------------------------
    [Header("Movement")]
    private Vector2 _moveInput;
    [SerializeField] private float _movementSpeed = 7.5f;
    [SerializeField] private float _smoothTime = 0.1f;
    private float _turnSmoothVelocity;
    public float _speedChangeRate = 50;
    private float speed;
    private float _animationSpeed;
    private float targetAngle;
//----------------------------------------------------------------------------------------------------------------
    [Header("Ground Sensor")]
    [SerializeField] private Transform _sensor;
    [SerializeField] float _sensorRadius = 0.5f;
    [SerializeField] private LayerMask _groundLayer;
//----------------------------------------------------------------------------------------------------------------
    [Header("Jump & Gravity")]
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private Vector3 _playerGravity;
    [SerializeField] private float _gravity = -15f;
    public float jumpTimeOut = 0.05f;
    public float fallTimeOut = 0.15f;
    float _jumpTimeOutDelta;
    float _fallTimeOutDelta;
//----------------------------------------------------------------------------------------------------------------
    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 30;
    [SerializeField] private float _dashTime = 0.25f;
    private Vector3 _lastMoveDirection;
    private bool isDashing = false;
//----------------------------------------------------------------------------------------------------------------
    [Header("Dash Cooldown")]
    [SerializeField] private float dashCooldown = 1.25f;
    private bool isDashOnCooldown = false;
//----------------------------------------------------------------------------------------------------------------
    [Header("Camera")]
    [SerializeField] private Vector2 _lookInput;
    [SerializeField] private float _cameraSensitivity = 10;
    float _xRotation;
    [SerializeField] Transform _lookAtCamera;
    private Transform _mainCamera;
    public bool isToggled = false;
    [SerializeField] private CinemachineCamera _thirdPersonCamera;
    [SerializeField] private CinemachineCamera _aimCamera;
    private bool isCameraBlending;
//----------------------------------------------------------------------------------------------------------------
    [Header("Push")]
    [SerializeField] private float _pushForce = 2;
//----------------------------------------------------------------------------------------------------------------
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _dashAction = InputSystem.actions["Sprint"];
        _lookAction = InputSystem.actions["Look"];
        _aimAction = InputSystem.actions["Aim"];

        _mainCamera = Camera.main.transform;
        _thirdPersonCamera.Prioritize();
    }

    void Start()
    {
        _jumpTimeOutDelta = jumpTimeOut;
        _fallTimeOutDelta = fallTimeOut;
    }

    void Update()
    {
        _lookInput = _lookAction.ReadValue<Vector2>();

        if (isCameraBlending)
        {
            Gravity();
            return; 
        }

        if(_aimAction.WasPressedThisFrame())
        {
            ToggleCamera();
        }

        _moveInput = _moveAction.ReadValue<Vector2>();
        _animator.SetFloat("Horizontal", _moveInput.x);
        _animator.SetFloat("Vertical", _moveInput.y);

        

        Gravity();

        if(!isToggled)
        {
            Movement();
        }
        else
        {
            AimMovement();
        }
        
        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }

        if(_dashAction.WasPressedThisFrame() && _moveInput != Vector2.zero && !isDashing && !isDashOnCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    void ToggleCamera()
    {
        isToggled = !isToggled;
        if(isToggled)
        {
            transform.rotation = Quaternion.Euler(0, _mainCamera.eulerAngles.y, 0);
            _xRotation = _lookAtCamera.localEulerAngles.x;

            if (_xRotation > 180f) _xRotation -= 360f;

            _aimCamera.Prioritize();
        }
        else
        {
            float playerYaw = transform.eulerAngles.y;

            _mainCamera.rotation = Quaternion.Euler(_mainCamera.eulerAngles.x, playerYaw, 0);

            _turnSmoothVelocity = 0f;

            _thirdPersonCamera.Prioritize();
        }

        StartCoroutine(CameraBlendLock(0.5f));
    }

    IEnumerator CameraBlendLock(float duration)
    {
        isCameraBlending = true;
        yield return new WaitForSeconds(duration);
        isCameraBlending = false;
    }


    void Movement()
    {
        if(isDashing) return;

        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);
        float targetSpeed = _movementSpeed;

        if(direction == Vector3.zero)
        {
            targetSpeed = 0;
        }

        float currentSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        if(currentSpeed < targetSpeed || currentSpeed > targetSpeed)
        {
            speed = Mathf.MoveTowards(speed, targetSpeed * direction.magnitude, _speedChangeRate * Time.deltaTime);
        }
        else
        {
            speed = targetSpeed;
        }

        _animationSpeed = Mathf.Lerp(_animationSpeed, targetSpeed, _speedChangeRate * Time.deltaTime);

        if(_animationSpeed < 0.05f)
        {
            _animationSpeed = 0;
        }

        _animator.SetFloat("Speed", _animationSpeed);
        
        if (direction != Vector3.zero)
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            float horizontalSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
            bool isIdle = horizontalSpeed < 0.1f && speed < 0.1f;

            if(isIdle)
            {
                transform.rotation = Quaternion.Euler(0, targetAngle, 0);
                _turnSmoothVelocity = 0f;
            }
            else
            {
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
                transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            }

            _lastMoveDirection = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;
        }

        Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
        _controller.Move(moveDirection.normalized * (speed * Time.deltaTime) + _playerGravity * Time.deltaTime);
    }

    void AimMovement()
{
    //if(isCameraBlending) return;

    Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

    float mouseX = _lookInput.x * _cameraSensitivity * Time.deltaTime;
    float mouseY = _lookInput.y * _cameraSensitivity * Time.deltaTime;

    _xRotation -= mouseY;
    _xRotation = Mathf.Clamp(_xRotation, -89, 89);

    transform.Rotate(Vector3.up, mouseX);
    _lookAtCamera.localRotation = Quaternion.Euler(_xRotation, 0, 0);

    Vector3 move = Vector3.zero;

    if(direction != Vector3.zero)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
        move = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
        move.Normalize();

        _lastMoveDirection = move;
    }

    _controller.Move(move * (_movementSpeed * Time.deltaTime) +_playerGravity * Time.deltaTime);
}


    IEnumerator Dash()
    {
        isDashing = true;

        _animator.SetTrigger("Dash");
        
        float timer = 0;
        
        while(timer < _dashTime)
        {
            _controller.Move(_lastMoveDirection.normalized * _dashSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }
        
        isDashing = false;
        StartCoroutine(DashCoolDown());
    }

    IEnumerator DashCoolDown()
    {
        isDashOnCooldown = true;
        yield return new WaitForSecondsRealtime(dashCooldown);
        isDashOnCooldown = false;
    }

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
        _animator.SetBool("Grounded", IsGrounded());
        if(IsGrounded())
        {
            _fallTimeOutDelta = fallTimeOut;

            _animator.SetBool("Jump", false);
            _animator.SetBool("Fall", false);

            if(_playerGravity.y < 0)
            {
                _playerGravity.y = -2;
            }

            if(_jumpTimeOutDelta >= 0)
            {
                _jumpTimeOutDelta -= Time.deltaTime;
            }
        }
        
        else
        {
            _jumpTimeOutDelta = jumpTimeOut;

            if(_fallTimeOutDelta >= 0)
            {
                _fallTimeOutDelta -= Time.deltaTime;
            }
            else
            {
                _animator.SetBool("Fall", true);
            }

            _playerGravity.y += _gravity * Time.deltaTime;
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.gameObject.tag == "Pushable")
        {
            //Rigidbody rBody = hit.transform.GetComponent<Rigidbody>();

            Rigidbody rBody = hit.collider.attachedRigidbody;
            if (rBody == null || rBody.isKinematic)
            {
                return;
            }

            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            rBody.linearVelocity = pushDirection * _pushForce / rBody.mass;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
}