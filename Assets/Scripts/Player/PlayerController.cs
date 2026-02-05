using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController _controller;
    private Animator _animator;

    //Inputs
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _lookAction;
    private InputAction _toggle;

    [Header("Movement")]
    [SerializeField] private float _movementSpeed = 15;
    [SerializeField] private float _smoothTime = 0.1f;
    private float _turnSmoothVelocity;
}
