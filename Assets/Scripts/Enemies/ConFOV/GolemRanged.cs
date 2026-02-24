using UnityEngine;

public class GolemRanged : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private Vector3 originPoint; // Para que no te persiga por todo el mapa

    public enum State { Wandering, Chase, Attack, Retreat }
    public State currentState;

    [Header("Movement & Ranges")]
    public float wanderingRadius = 6f;
    public float maxDistanceDelta = 20f; // Distancia máxima desde su origen antes de rendirse
    public float chaseRange = 15f;
    public float attackRange = 10f;
    public float safeDistance = 5f;
    public float rotationSpeed = 15f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    [Header("Detection & FOV")]
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;
    public float viewAngle = 90f;

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
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        originPoint = transform.position; // Guardamos su "casa"

        agent.updateRotation = false; 
        agent.acceleration = 60f; 
        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceToOrigin = Vector3.Distance(transform.position, originPoint);

        // Lógica de "Me rindo": Si se aleja mucho de su origen, vuelve
        if (distanceToOrigin > maxDistanceDelta && currentState != State.Wandering)
        {
            ReturnToOrigin();
        }

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
        agent.stoppingDistance = 0.2f;

        // Solo persigue si LO VE dentro del FOV y Raycast
        if (CanSeePlayer(dist)) 
        { 
            currentState = State.Chase; 
            return; 
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            if (!isWaiting) { isWaiting = true; waitTimer = 0f; }
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime) { isWaiting = false; PickRandomPoint(); }
        }
    }

    private void UpdateChase(float dist)
    {
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        // Si te pierde de vista (te escondes), vuelve a Wandering tras un momento
        if (!CanSeePlayer(dist))
        {
            ReturnToOrigin();
            return;
        }

        if (dist <= attackRange) currentState = State.Attack;
    }

    private void UpdateAttack(float dist)
    {
        agent.isStopped = true;

        // Si el jugador se esconde tras una pared mientras apunta
        if (!CanSeePlayer(dist))
        {
            currentState = State.Chase;
            return;
        }
        
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

        Vector3 dirToPlayer = transform.position - player.position;
        Vector3 retreatPos = transform.position + dirToPlayer.normalized * 5f;

        agent.SetDestination(retreatPos);

        if (dist > safeDistance + 2f) currentState = State.Attack;
    }

    // --- SISTEMA DE VISIÓN ---
    bool CanSeePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > chaseRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < viewAngle / 2f)
        {
            Vector3 startPos = transform.position + (Vector3.up * eyeHeight) + (transform.forward * 0.5f);
            Vector3 targetPos = player.position + Vector3.up * eyeHeight;
            Vector3 direction = (targetPos - startPos).normalized;

            // Dibujamos el rayo en la ventana Scene para verlo físicamente
            Debug.DrawRay(startPos, direction * chaseRange, Color.red);

            RaycastHit hit;
            // Lanzamos el rayo SIN el obstacleMask para ver con qué choca primero
            if (Physics.Raycast(startPos, direction, out hit, chaseRange))
            {
                // ESTO ES LO IMPORTANTE: Mira la consola de Unity
                Debug.Log("El rayo del Golem está golpeando a: " + hit.collider.name + " con el Tag: " + hit.collider.tag);

                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
            else
            {
                Debug.Log("El rayo no ha golpeado NADA");
            }
        }
        return false;
    }

    private void ReturnToOrigin()
    {
        currentState = State.Wandering;
        agent.SetDestination(originPoint);
    }

    private void ApplyManualRotation()
    {
        Vector3 targetDir = (currentState != State.Wandering) ? 
            (player.position - transform.position) : agent.velocity;

        if (targetDir.sqrMagnitude > 0.1f)
        {
            targetDir.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }
    }

    public void LaunchProjectile()
    {
        if(projectilePrefab && shootPoint)
            Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
    }

    void PickRandomPoint()
    {
        // Wandering centrado en su origen, no en sí mismo (para delimitar zona)
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += originPoint;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out UnityEngine.AI.NavMeshHit hit, wanderingRadius, UnityEngine.AI.NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void UpdateAnimator() => animator.SetFloat("Speed", agent.velocity.magnitude);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(originPoint, maxDistanceDelta); // Rango máximo de persecución
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originPoint, wanderingRadius); // Zona de paseo

        // Visualización FOV
        Gizmos.color = Color.white;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, left * chaseRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, right * chaseRange);
    }
}