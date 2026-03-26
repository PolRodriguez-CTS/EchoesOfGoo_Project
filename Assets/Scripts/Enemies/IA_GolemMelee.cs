using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof (Rigidbody))]
public class IA_GolemMelee : MonoBehaviour, IAtacante, IKnockbackeable
{
    private NavMeshAgent _agent;
    private Transform player;
    private Animator animator;
    private Rigidbody _rigidBody;
    private Vector3 originPoint;
    public enum State { Wandering, Chase, Attack }
    public State currentState;

    [Header("Movement")]
    public float wanderingRadius = 7f;
    public float maxDistanceDelta = 25f; // Distancia máxima desde el origen
    public float chaseRange = 15f;
    public float attackRange = 3f; 
    public float rotationSpeed = 15f; // Giro rápido y fluido
    public float walkSpeed = 2f;
    public float runSpeed = 4.0f;

    [Header("Detection & FOV")]
    public float eyeHeight = 1.6f;
    public float viewAngle = 100f;
    public float forwardOffset = 0.6f; // Para saltarse su propia colisión

    [Header("Wandering Settings")]
    public float waitTime = 2f;
    private float waitTimer;
    private bool isWaiting;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    private float attackTimer;
    public float stoppingDistanceBuffer = 0.5f;
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private float attackHitboxRange;
    [SerializeField] private float attackDamage;

    [Header("Combo Settings")]
    public int comboStep = 0;
    public float comboResetTime = 3.5f;
    private float lastAttackTime;

    [Header ("Knockback")]
    [Range(0.001f, 0.1f)] [SerializeField] private float stillThreshold = 0.05f;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        originPoint = transform.position;
    }

    void Start()
    {
        // Configuración para evitar conflictos de rotación y permitir frenado en seco
        _agent.updateRotation = false; 
        _agent.acceleration = 100f; 

        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distToOrigin = Vector3.Distance(transform.position, originPoint);
        
        if (distToOrigin > maxDistanceDelta && currentState != State.Wandering)
            {
                ReturnToOrigin();
            }

        switch (currentState)
        {
            case State.Wandering:
                UpdateWanderingState(distanceToPlayer);
                break;
            case State.Chase:
                UpdateChaseState(distanceToPlayer);
                break;
            case State.Attack:
                UpdateAttackState(distanceToPlayer);
                break;
        }

        // Aplicamos la rotación manual para que sea instantánea al cambiar de dirección
        ApplyManualRotation();
        
        // Actualizamos las animaciones
        UpdateAnimator();
    }

    private void UpdateWanderingState(float distanceToPlayer)
    {
        _agent.isStopped = false;
        _agent.speed = walkSpeed;
        _agent.stoppingDistance = 0.2f; // Permitir que llegue al punto exacto

        // Detectar jugador
        if (CanSeePlayer(distanceToPlayer))
        {
            isWaiting = false;
            currentState = State.Chase;
            return;
        }

        // Lógica de patrulla con espera
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
            }

            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                PickRandomPoint();
            }
        }
    }

    private void UpdateChaseState(float distanceToPlayer)
    {
        _agent.isStopped = false;
        _agent.speed = runSpeed;
        _agent.stoppingDistance = attackRange - 0.5f; 
        _agent.SetDestination(player.position);

        // Atacar de inmediato si entra en rango
        if (distanceToPlayer <= attackRange)
        {
            StartAttackSequence();
        }

        // Perder al jugador
        if (!CanSeePlayer(distanceToPlayer) && distanceToPlayer > attackRange + 2f)
        {
            isWaiting = false;
            ReturnToOrigin();
        }
    }

    private void UpdateAttackState(float distanceToPlayer)
    {
        // Clavar los pies mientras esté en rango de ataque
        if (distanceToPlayer <= attackRange)
            _agent.isStopped = true;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            Attack();
            attackTimer = 0f;
        }

        // Si el jugador se aleja lo suficiente, volver a perseguir
        if (distanceToPlayer > attackRange + stoppingDistanceBuffer)
        {
            _agent.isStopped = false;
            currentState = State.Chase;
        }
    }

    private void ApplyManualRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        if (currentState == State.Attack)
        {
            targetDirection = (player.position - transform.position).normalized;
        }
        else if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            targetDirection = _agent.velocity.normalized;
        }

        if (targetDirection != Vector3.zero)
        {
            targetDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void StartAttackSequence()
    {
        // Esta función fuerza el primer golpe sin esperar al timer
        animator.SetInteger("ComboIndex", comboStep);
        animator.SetTrigger("Attack"); 
        
        attackTimer = 0f; 
        currentState = State.Attack;
    }

    void PickRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderingRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    bool CanSeePlayer(float dist)
{
    if (dist > chaseRange) return false;

    Vector3 dirToPlayer = (player.position - transform.position).normalized;

    // USAMOS EL ÁNGULO
    float angle = Vector3.Angle(transform.forward, dirToPlayer);

    if (angle < viewAngle / 2f)
    {
        // AJUSTE CRÍTICO: El targetPos debe ser el centro del Player, no los pies.
        // Si player.position es la base (pies), el rayo al suelo suele fallar.
        Vector3 startPos = transform.position + (Vector3.up * eyeHeight) + (transform.forward * forwardOffset);
        
        // Apuntamos a 1 metro sobre la base del player (el pecho/cabeza)
        Vector3 targetPos = player.position + Vector3.up * 1.2f; 
        Vector3 direction = (targetPos - startPos).normalized;

        RaycastHit hit;

        // Lanzamos el rayo
        if (Physics.Raycast(startPos, direction, out hit, chaseRange))
        {
            // Debug para ver el rayo en tiempo real
            Debug.DrawLine(startPos, hit.point, Color.red);

            if (hit.collider.CompareTag("Player")) 
            {
                return true; 
            }
            else 
            {
                // Si choca con otra cosa, imprime qué es para saber qué lo bloquea
                // Debug.Log("Bloqueado por: " + hit.collider.name);
            }
        }
    }
    return false;
}

    void Attack()
    {
        if (animator == null) return;

        // Reset del combo si pasó mucho tiempo
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        // Disparar animación
        animator.ResetTrigger("Attack");
        animator.SetInteger("ComboIndex", comboStep);
        animator.SetTrigger("Attack");

        lastAttackTime = Time.time;
        
        // Siguiente paso del combo
        comboStep = (comboStep + 1) % 2;
    }

    void ReturnToOrigin() { currentState = State.Wandering; _agent.SetDestination(originPoint); }

    public void PlayerDamage()
    {
        Collider[] reachedObjects = Physics.OverlapSphere(attackHitbox.position, attackHitboxRange);
        foreach(Collider col in reachedObjects)
        {
            if(col.CompareTag("Player"))
            {
                PlayerHealth _playerHealthScript = col.gameObject.GetComponent<PlayerHealth>();
                if(_playerHealthScript != null)
                {
                    _playerHealthScript.Damaged(attackDamage);
                }
            }
        }
    }

    public void GetKnockedBack(Vector3 force)
    {
        //StartCoroutine(ApplyKnockback(force));
    }

    /*private IEnumerator ApplyKnockback(Vector3 force)
    {
        yield return null;
        _agent.isStopped = true;
        _agent.enabled = false;

        _rigidBody.useGravity = true;
        _rigidBody.isKinematic = false;
        _rigidBody.AddForce(force);

        yield return new WaitForFixedUpdate();
        float knockBackTime = Time.time;
        yield return new WaitUntil(() => _rigidBody.linearVelocity.magnitude < stillThreshold);
        yield return new WaitForSeconds(0.25f);

        _rigidBody.linearVelocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;
        _rigidBody.useGravity = false;
        _rigidBody.isKinematic = true;

        _agent.Warp(transform.position);
        _agent.enabled = true;

        yield return null;
    }*/

    void UpdateAnimator()
    {
        if (animator == null) return;
        // El parámetro "Speed" debe mover el Blend Tree de Idle a Run
        animator.SetFloat("Speed", _agent.velocity.magnitude);
    }

    // Opcional: Para ver el rango en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderingRadius);

        //FOV
        Gizmos.color = Color.white;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, left * chaseRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, right * chaseRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackHitbox.position, attackHitboxRange);
    }

    public void ImpulsoTesteo(Vector3 fuerza)
    {
        _agent.enabled = false;
        _agent.isStopped = true;

        _rigidBody.isKinematic = false;
        _rigidBody.useGravity = true;
        _rigidBody.AddForce(fuerza, ForceMode.Impulse);
    }
}