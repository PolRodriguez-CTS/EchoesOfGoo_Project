using UnityEngine;

public class GolemMelee : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private Vector3 originPoint;

    public enum State { Wandering, Chase, Attack }
    [Header("Status")]
    public State currentState;

    [Header("Movement & Ranges")]
    public float wanderingRadius = 7f;
    public float maxDistanceDelta = 25f; // Distancia máxima desde el origen
    public float chaseRange = 12f;
    public float attackRange = 2.5f;
    public float rotationSpeed = 15f;
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;

    [Header("Detection & FOV")]
    public float eyeHeight = 1.6f;
    public float viewAngle = 100f;
    public float forwardOffset = 0.6f; // Para saltarse su propia colisión

    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    private float attackTimer;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        originPoint = transform.position;

        agent.updateRotation = false;
        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        float distToOrigin = Vector3.Distance(transform.position, originPoint);

        if (distToOrigin > maxDistanceDelta && currentState != State.Wandering)
        {
            ReturnToOrigin();
        }

        switch (currentState)
        {
            case State.Wandering: UpdateWandering(distToPlayer); break;
            case State.Chase:     UpdateChase(distToPlayer); break;
            case State.Attack:    UpdateAttack(distToPlayer); break;
        }

        ApplyManualRotation();
        if (animator) animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    void UpdateWandering(float dist)
    {
        //-----------------------------------------------------------------------
        agent.speed = walkSpeed;
        if (CanSeePlayer(dist)) { currentState = State.Chase; return; }

        if (!agent.pathPending && agent.remainingDistance <= 0.3f) PickRandomPoint();
    }

    void UpdateChase(float dist)
    {
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        if (dist <= attackRange) currentState = State.Attack;
        if (!CanSeePlayer(dist) && dist > attackRange + 2f) ReturnToOrigin();
    }

    void UpdateAttack(float dist)
    {
        agent.isStopped = true;
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            animator.SetTrigger("Attack");
            attackTimer = 0;
        }

        if (dist > attackRange + 0.5f) currentState = State.Chase;
    }

    bool CanSeePlayer(float dist)
    {
        if (dist > chaseRange) return false;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
        {
            Vector3 startPos = transform.position + (Vector3.up * eyeHeight) + (transform.forward * forwardOffset);
            Vector3 targetPos = player.position + Vector3.up * eyeHeight;
            RaycastHit hit;
            if (Physics.Raycast(startPos, (targetPos - startPos).normalized, out hit, chaseRange))
            {
                if (hit.collider.CompareTag("Player")) return true;
            }
        }
        return false;
    }

    void ReturnToOrigin() { currentState = State.Wandering; agent.SetDestination(originPoint); }

    void ApplyManualRotation()
    {
        Vector3 targetDir = (currentState != State.Wandering) ? (player.position - transform.position) : agent.velocity;
        if (targetDir.sqrMagnitude > 0.1f)
        {
            targetDir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * rotationSpeed);
        }
    }

    void PickRandomPoint()
    {
        Vector3 randDir = Random.insideUnitSphere * wanderingRadius + originPoint;
        if (UnityEngine.AI.NavMesh.SamplePosition(randDir, out UnityEngine.AI.NavMeshHit hit, wanderingRadius, 1)) agent.SetDestination(hit.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, left * chaseRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, right * chaseRange);
    }
}