using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IA_GolemRanged : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    public enum State
    {
        Wandering,
        MantainDistance,
        Attack
    }

    public State currentState;

    [Header("Ranges")]
    public float wanderingRadius = 6f; //Rango maximo del siguiente paseo
    public float detectRange = 8f; //Rango en el que empieza a atacar al jugador
    public float securityDistance = 6f; //Distancia de seguridad

    [Header("Distance")]
    

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    private float attackTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();

        currentState = State.Wandering;
        //PickRandomPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Wandering:

                //detectar al player y cambiar a trackear
                if (distanceToPlayer < detectRange)
                {
                    currentState = State.Attack;
                }

                if(distanceToPlayer < securityDistance)
                {
                    currentState = State.MantainDistance;
                }

                // Si llegó al destino, elige otro punto
                if (!agent.pathPending && agent.remainingDistance < 0.2f)
                {
                    PickRandomPoint();
                }

                break;

            case State.MantainDistance:

                agent.SetDestination(player.position);

                Vector3 dir = (transform.position - player.position).normalized;

                transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, agent.velocity.magnitude * Time.deltaTime);

                if (distanceToPlayer > detectRange)
                {
                    currentState = State.Wandering;
                    PickRandomPoint();
                }

                if(distanceToPlayer > securityDistance)
                {
                    currentState = State.Attack;
                }

            break;

            case State.Attack:

                agent.SetDestination(transform.position); //Se queda quieto

                attackTimer += Time.deltaTime;

                if (attackTimer >= attackCooldown)
                {
                    Attack();
                    attackTimer = 0f;
                }

                //Si se inclumple la distancia de seguridad cambia de estado
                if (distanceToPlayer < securityDistance)
                {
                    currentState = State.MantainDistance;
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
