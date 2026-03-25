using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IA_GolemRanged : MonoBehaviour, IAtacante, ILasear
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private Rigidbody _rigidBody;
    private Vector3 originPoint;

    public enum State { Wandering, Chase, Attack, Retreat }
    [Header("Status")]
    public State currentState;

    [Header("Movement & Ranges")]
    public float wanderingRadius = 6f;
    public float maxDistanceDelta = 25f; // Límite para que no huya al infinito
    public float chaseRange = 15f;    
    public float attackRange = 10f;   
    public float safeDistance = 5f;   
    public float rotationSpeed = 15f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    [Header("Detection & FOV")]
    public float eyeHeight = 1.6f;
    public float viewAngle = 200;
    public float forwardOffset = 0.6f; 

    [Header("Ranged Attack")]
    public GameObject laserBeam;
    public Transform shootPoint;
    private float attackDamage = 30;
    public float fireRate = 2f;
    private float fireTimer;

    [Header("Wandering Settings")]
    public float waitTime = 2f;
    private float waitTimer;
    private bool isWaiting;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        originPoint = transform.position;

        agent.updateRotation = false; 
        agent.acceleration = 60f; 
        
        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceToOrigin = Vector3.Distance(transform.position, originPoint);

        // Si se aleja demasiado de su casa por estar huyendo o persiguiendo, vuelve.
        if (distanceToOrigin > maxDistanceDelta && currentState != State.Wandering)
        {
            ReturnToOrigin();
        }

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

            case State.Retreat:
            UpdateRetreat(distanceToPlayer);
            break;
        }

        ApplyManualRotation();
        UpdateAnimator();
    }

    private void UpdateWandering(float dist)
    {
        agent.speed = walkSpeed;

        // CAMBIO: Ahora solo detecta si CanSeePlayer es verdadero (Ángulo + Raycast)
        if (CanSeePlayer(dist)) 
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
        
        // Si lo pierde de vista (paredes o espalda), vuelve a Wandering
        if (!CanSeePlayer(dist) && dist > attackRange + 2f) ReturnToOrigin();
    }

    private float laserTime = 0.5f;
    private float laserTimer;
    private void UpdateAttack(float dist)
    {
        agent.isStopped = true;
        
        // Si el jugador se esconde tras una columna mientras el Golem apunta
        if (!CanSeePlayer(dist))
        {
            currentState = State.Chase;
            return;
        }

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            laserTimer += Time.deltaTime;
            



            if(laserTimer >= laserTime)
            {
                animator.SetTrigger("Attack");
                fireTimer = 0;
                laserTimer = 0;
            }
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

    // EL SISTEMA DE VISIÓN ADAPTADO
    bool CanSeePlayer(float dist)
    {
        if (dist > chaseRange) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle < viewAngle / 2f)
        {
            Vector3 startPos = transform.position + (Vector3.up * eyeHeight) + (transform.forward * forwardOffset);
            Vector3 targetPos = player.position + Vector3.up * 1.2f; // Apunta al pecho
            Vector3 direction = (targetPos - startPos).normalized;

            RaycastHit hit;
            if (Physics.Raycast(startPos, direction, out hit, chaseRange))
            {
                Debug.DrawLine(startPos, hit.point, Color.cyan); // Línea cian en Scene

                if (hit.collider.CompareTag("Player")) return true;
            }
        }
        return false;
    }

    private void ApplyManualRotation()
    {
        Vector3 targetDir;
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

    public void Laser()
    {
        if (laserBeam != null && shootPoint != null)
        {
        // 1. Instanciamos y guardamos la referencia
        GameObject tempLaser = Instantiate(laserBeam, shootPoint.position, shootPoint.rotation);
        
        // 2. Lógica de DAÑO inmediata (Raycast)
        RaycastHit hit;
        // Usamos la misma lógica que tu CanSeePlayer pero para hacer daño
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, attackRange - 1))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Buscamos el script de vida del jugador
                PlayerHealth _playerHealthScript = hit.collider.GetComponent<PlayerHealth>();
                if (_playerHealthScript != null)
                {
                    _playerHealthScript.Damaged(attackDamage);
                }
            }
        }
            // 3. Destruimos la COPIA (tempLaser), no el prefab (laserBeam)
            Destroy(tempLaser, 1.5f);
        }
    }

    void PickRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += originPoint; // Wandering alrededor de su zona
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderingRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    void ReturnToOrigin() { currentState = State.Wandering; agent.SetDestination(originPoint); }

    void UpdateAnimator()
    {
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar FOV en Scene
        Gizmos.color = Color.white;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, left * chaseRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, right * chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
    }
}