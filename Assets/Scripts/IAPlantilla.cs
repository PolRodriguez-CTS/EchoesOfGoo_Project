using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IAPlantilla : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    public enum State
    {
        Wandering,
        Chase,
        Attack
    }

    public State currentState;

    [Header("Movement")]
    public float wanderingRadius = 6f;
    public float chaseRange = 8f;
    public float attackRange = 2f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    private float attackTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();

        currentState = State.Wandering;
        PickRandomPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Wandering:

                if (distanceToPlayer < chaseRange)
                {
                    currentState = State.Chase;
                }

                // Si llegó al destino, elige otro punto
                if (!agent.pathPending && agent.remainingDistance < 0.2f)
                {
                    PickRandomPoint();
                }

                break;

            case State.Chase:

                agent.SetDestination(player.position);

                if (distanceToPlayer < attackRange)
                {
                    currentState = State.Attack;
                }

                if (distanceToPlayer > chaseRange)
                {
                    currentState = State.Wandering;
                    PickRandomPoint();
                }

                break;

            case State.Attack:

                agent.SetDestination(transform.position); // Se queda quieto

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

        UpdateAnimator();
    }

    void PickRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderingRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void Attack()
    {
        Debug.Log("Enemy attacks!");

        if (animator != null)
            animator.SetTrigger("Attack");
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
}
