using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IALoglin : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    public enum State
    {
        Wandering,
        Chase,
        Attack
    }

    public State currentState;

    [Header("Wandering")]
    public float wanderingRadius = 6f;
    public float minimumDistance = 3f;

    [Header("Aggro / Chase")]
    public float chaseRange = 8f;
    public float chaseTime = 5f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private bool isAggro = false;
    private float chaseTimer = 0f;
    private float attackTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;

        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            // ---------------- WANDERING ----------------
            case State.Wandering:

                if (isAggro && distanceToPlayer < chaseRange)
                {
                    currentState = State.Chase;
                    chaseTimer = 0f;
                }

                if (!agent.pathPending && agent.remainingDistance < 0.2f)
                {
                    PickRandomPoint();
                }

                break;

            // ---------------- CHASE ----------------
            case State.Chase:

                agent.SetDestination(player.position);

                chaseTimer += Time.deltaTime;

                if (distanceToPlayer < attackRange)
                {
                    currentState = State.Attack;
                }

                if (chaseTimer >= chaseTime)
                {
                    isAggro = false;
                    currentState = State.Wandering;
                    PickRandomPoint();
                }

                break;

            // ---------------- ATTACK ----------------
            case State.Attack:

                agent.SetDestination(transform.position); 

                attackTimer += Time.deltaTime;

                if (attackTimer >= attackCooldown)
                {
                    Attack();
                    attackTimer = 0f;
                }

                if (distanceToPlayer > attackRange)
                {
                    currentState = State.Chase;
                }

                break;
        }
    }

    void PickRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderingRadius, NavMesh.AllAreas))
        {
            // Asegura que no sea demasiado cerca
            if (Vector3.Distance(transform.position, hit.position) > minimumDistance)
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    void Attack()
    {
        Debug.Log("Loglin ataca");
        // Aquí iría animación + daño
    }

    // Llamar esto desde el arma del jugador
    public void TakeDamage()
    {
        isAggro = true;
    }
}

