using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IALoglin : MonoBehaviour, IRageable, IAtacante, IKnockbackeable
{
    private NavMeshAgent _agent;
    private Transform player;
    private Animator animator;

    public enum State { Wandering, Chase, Attack, Stunned }
    [Header("Estado Actual")]
    public State currentState;
    public bool hasBeenHit = false; // El interruptor de agresividad

    [Header("Rangos")]
    public float wanderingRadius = 6f;
    public float chaseRange = 10f;
    public float attackRange = 2.5f;
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.0f;

    [Header("Ataque")]
    public float attackCooldown = 1.5f;
    private float attackTimer;

    [Header("Patrulla")]
    public float waitTime = 2f;
    private float waitTimer;
    private bool isWaiting;

    [Header("Attack Settings")]
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private float attackHitboxRange;
    [SerializeField] private float attackDamage;

    //temporal hasta implementar stun aquí
    public bool IsStunned => currentState == State.Stunned;
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();

        _agent.updateRotation = true; 
        // TIP: Ajusta el stoppingDistance para que no colisionen físicamente
        _agent.stoppingDistance = attackRange; 
        
        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Wandering: 
            UpdateWandering(distanceToPlayer); 
            break;
            
            case State.Chase:     
            UpdateChase(distanceToPlayer); 
            break;
            
            case State.Attack:    
            UpdateAttack(distanceToPlayer); 
            break;
            
            case State.Stunned:
            HandleStun();
            break;
        }

        UpdateAnimator();
    }

    private void UpdateWandering(float dist)
    {
        _agent.speed = walkSpeed;

    // 1. Detección de jugador (Mantenemos tu lógica de hasBeenHit)
    if (hasBeenHit && dist < chaseRange)
    {
        isWaiting = false; // Resetear por si acaso
        currentState = State.Chase;
        return;
    }

    // 2. Lógica de Patrulla
    // Añadimos comprobación de que el agente no esté calculando el camino
    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
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

    private void UpdateChase(float dist)
    {
        _agent.isStopped = false; // Nos aseguramos de que pueda moverse
        _agent.speed = runSpeed;
        _agent.SetDestination(player.position);

        if (dist <= attackRange) 
        {
            currentState = State.Attack;
        }

        if (dist > chaseRange + 5f) currentState = State.Wandering;
    }

    private void UpdateAttack(float dist)
    {
        // 1. Mantener el destino actualizado pero dejar que stoppingDistance haga su magia
        _agent.SetDestination(player.position);

        // 2. Rotación manual: Si está cerca y parado, que siga mirando al jugador
        if (dist <= _agent.stoppingDistance + 0.5f)
        {
            LookAtPlayer();
        }

        // 3. Lógica de ataque
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown && dist <= attackRange)
        {
            animator.SetTrigger("Attack");
            attackTimer = 0f;
        }

        // 4. Volver a Chase si el jugador se escapa
        if (dist > attackRange + 0.5f) 
        {
            currentState = State.Chase;
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Evitamos que el enemigo se incline hacia arriba/abajo
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    // --- FUNCIÓN PÚBLICA PARA ACTIVARLO ---
    // Llama a esta función desde tu script de "Vida" o "Daño"
    public void Raged()
    {
        hasBeenHit = true;
        currentState = State.Chase;
        Debug.Log("¡Enemigo pasivo provocado!");
    }

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

    void PickRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderingRadius, 1))
        {
            _agent.SetDestination(hit.position);
        }
    }

    float stunTimer;
    void HandleStun()
    {
        stunTimer -= Time.deltaTime;
        //si llega a cero se reactiva el agent y cambia de estado
        if(stunTimer <= 0)
        {
            _agent.enabled = true;
            currentState = State.Chase;
        }
    }

    public void GetKnockedBack(Vector3 force, float duration)
    {
        currentState = State.Stunned;
        stunTimer = duration;
        _agent.enabled = false;
        StartCoroutine(ApplyKnockback(force, duration));
    }

    private IEnumerator ApplyKnockback(Vector3 force, float duration)
    {
        float elapsed = 0;
        while(elapsed < duration)
        {
            Vector3 knockForce = Vector3.Lerp(force, Vector3.zero, elapsed / duration);
            transform.position += knockForce * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
            animator.SetFloat("Speed", _agent.velocity.magnitude);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackHitbox.position, attackHitboxRange);
    }
}