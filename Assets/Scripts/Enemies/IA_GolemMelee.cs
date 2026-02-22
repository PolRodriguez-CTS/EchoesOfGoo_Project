using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IA_GolemMelee : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    public enum State { Wandering, Chase, Attack }
    public State currentState;

    [Header("Movement")]
    public float wanderingRadius = 6f;
    public float chaseRange = 8f;
    public float attackRange = 3f; 
    public float rotationSpeed = 20f; // Giro rápido y fluido
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.0f;

    [Header("Wandering Settings")]
    public float waitTime = 2f;
    private float waitTimer;
    private bool isWaiting;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    private float attackTimer;
    public float stoppingDistanceBuffer = 0.5f; 

    [Header("Combo Settings")]
    public int comboStep = 0;
    public float comboResetTime = 3.5f;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();

        // Configuración para evitar conflictos de rotación y permitir frenado en seco
        agent.updateRotation = false; 
        agent.acceleration = 100f; 

        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

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
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0.2f; // Permitir que llegue al punto exacto

        // Detectar jugador
        if (distanceToPlayer < chaseRange)
        {
            isWaiting = false;
            currentState = State.Chase;
            return;
        }

        // Lógica de patrulla con espera
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = attackRange - 0.5f; 
        agent.SetDestination(player.position);

        // Atacar de inmediato si entra en rango
        if (distanceToPlayer <= attackRange)
        {
            StartAttackSequence();
        }

        // Perder al jugador
        if (distanceToPlayer > chaseRange)
        {
            isWaiting = false;
            currentState = State.Wandering;
            PickRandomPoint();
        }
    }

    private void UpdateAttackState(float distanceToPlayer)
    {
        // Clavar los pies mientras esté en rango de ataque
        if (distanceToPlayer <= attackRange)
            agent.isStopped = true;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            Attack();
            attackTimer = 0f;
        }

        // Si el jugador se aleja lo suficiente, volver a perseguir
        if (distanceToPlayer > attackRange + stoppingDistanceBuffer)
        {
            agent.isStopped = false;
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
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            targetDirection = agent.velocity.normalized;
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
            agent.SetDestination(hit.position);
        }
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
        comboStep = (comboStep + 1) % 3;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        // El parámetro "Speed" debe mover el Blend Tree de Idle a Run
        animator.SetFloat("Speed", agent.velocity.magnitude);
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
    }
}