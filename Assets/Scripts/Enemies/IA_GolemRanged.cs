using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IA_GolemRanged : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    public enum State { Wandering, Chase, Attack, Retreat }
    [Header("Status")]
    public State currentState;

    [Header("Movement & Ranges")]
    public float wanderingRadius = 6f;
    public float chaseRange = 15f;    // Distancia para empezar a perseguir
    public float attackRange = 10f;   // Distancia para disparar
    public float safeDistance = 5f;   // Si el jugador se acerca más, el Golem huye
    public float rotationSpeed = 15f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform shootPoint;      
    public float fireRate = 2f;
    private float fireTimer;

    [Header("Wandering Settings")]
    public float waitTime = 2f;
    private float waitTimer;
    private bool isWaiting;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();

        // Configuración para que el script controle la rotación manualmente
        agent.updateRotation = false; 
        agent.acceleration = 60f; 
        
        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Wandering: UpdateWandering(distanceToPlayer); break;
            case State.Chase:     UpdateChase(distanceToPlayer); break;
            case State.Attack:    UpdateAttack(distanceToPlayer); break;
            case State.Retreat:   UpdateRetreat(distanceToPlayer); break;
        }

        ApplyManualRotation();
        UpdateAnimator();
    }

    private void UpdateWandering(float dist)
    {
        agent.speed = walkSpeed;

        // Detección simple por distancia
        if (dist < chaseRange) 
        { 
            isWaiting = false;
            currentState = State.Chase; 
            return; 
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            if (!isWaiting) { isWaiting = true; waitTimer = 0f; }
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
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        if (dist <= attackRange) currentState = State.Attack;
        
        // Si el jugador se aleja demasiado, vuelve a deambular
        if (dist > chaseRange + 2f) currentState = State.Wandering;
    }

    private void UpdateAttack(float dist)
    {
        agent.isStopped = true;
        
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            animator.SetTrigger("Attack"); 
            fireTimer = 0;
        }

        if (dist < safeDistance) currentState = State.Retreat;
        if (dist > attackRange + 2f) currentState = State.Chase;
    }

    private void UpdateRetreat(float dist)
    {
        agent.isStopped = false;
        agent.speed = runSpeed;

        // Calcular punto de huida (dirección opuesta al jugador)
        Vector3 dirToPlayer = transform.position - player.position;
        Vector3 retreatPos = transform.position + dirToPlayer.normalized * 5f;

        agent.SetDestination(retreatPos);

        // Si ya está a una distancia segura, vuelve a atacar
        if (dist > safeDistance + 2f) currentState = State.Attack;
    }

    private void ApplyManualRotation()
    {
        Vector3 targetDir;

        // Si está peleando, mira al jugador. Si camina, mira hacia donde va.
        if (currentState != State.Wandering)
            targetDir = (player.position - transform.position);
        else
            targetDir = agent.velocity;

        if (targetDir.sqrMagnitude > 0.1f)
        {
            targetDir.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }
    }

    //Pendiente de optimizar
    public void LaunchProjectile()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        }
    }

    void PickRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderingRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}