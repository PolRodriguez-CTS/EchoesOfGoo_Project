using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IA_GolemRanged : MonoBehaviour, IAtacante, ILasear, IKnockbackeable
{
    private NavMeshAgent _agent;
    private Transform player;
    private Animator animator;
    private Rigidbody _rigidBody;
    private Vector3 originPoint;

    public enum State { Wandering, Chase, Attack, Retreat, Stunned }
    [Header("Status")]
    public State currentState;

    [Header("Movement & Ranges")]
    public float wanderingRadius = 6f;
    public float maxDistanceDelta = 25f; // Límite para que no huya al infinito
    public float chaseRange = 15f;    
    public float attackRange = 10f;   
    public float safeDistance = 5f;   
    public float rotationSpeed = 15f;
    public float walkSpeed = 4f;
    public float runSpeed = 8f;

    [Header("Detection & FOV")]
    public float eyeHeight = 1.6f;
    public float viewAngle = 200;
    public float forwardOffset = 0.6f; 

    [Header("Ranged Attack")]
    public GameObject laserBeam;
    public Transform shootPoint;
    private float attackDamage = 1;
    public float fireRate = 2f;
    private float fireTimer;

    [Header("Wandering Settings")]
    public float waitTime = 2f;
    private float waitTimer;
    private bool isWaiting;

    public bool IsStunned => currentState == State.Stunned;

    [Header("Explosion Settings")]
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _collisionForceThreshold = 10f; // Velocidad mínima para explotar
    [SerializeField] private int _explosionDamage = 50;
    [SerializeField] private float _explosionRadius = 5f;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        originPoint = transform.position;

        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.isKinematic = true;
    }

    void Start()
    {
        _agent.updateRotation = false; 
        _agent.acceleration = 60f; 
        
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

            case State.Stunned:
            HandleStun();
            break;
        }

        ApplyManualRotation();
        UpdateAnimator();
    }

    private void UpdateWandering(float dist)
    {
        _agent.speed = walkSpeed;

        // CAMBIO: Ahora solo detecta si CanSeePlayer es verdadero (Ángulo + Raycast)
        if (CanSeePlayer(dist)) 
        { 
            isWaiting = false;
            currentState = State.Chase; 
            return; 
        }

        if (!_agent.pathPending && _agent.remainingDistance <= 0.2f)
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
        _agent.isStopped = false;
        _agent.speed = runSpeed;
        _agent.SetDestination(player.position);

        if (dist <= attackRange) currentState = State.Attack;
        
        // Si lo pierde de vista (paredes o espalda), vuelve a Wandering
        if (!CanSeePlayer(dist) && dist > attackRange + 2f) ReturnToOrigin();
    }

    private float laserTime = 0.5f;
    private float laserTimer;
    private void UpdateAttack(float dist)
    {
        _agent.isStopped = true;
        
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
        _agent.isStopped = false;
        _agent.speed = runSpeed;

        Vector3 dirToPlayer = transform.position - player.position;
        Vector3 retreatPos = transform.position + dirToPlayer.normalized * 5f;

        _agent.SetDestination(retreatPos);

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
            targetDir = _agent.velocity;

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
            _agent.SetDestination(hit.position);
        }
    }

    void ReturnToOrigin() { currentState = State.Wandering; _agent.SetDestination(originPoint); }

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
        StopAllCoroutines();

        currentState = State.Stunned;
        stunTimer = duration;

        _agent.enabled = false;

        if(force.magnitude > 0.1f)
        {
            _rigidBody.isKinematic = false;
            _rigidBody.useGravity = true;

            _rigidBody.AddForce(force, ForceMode.Impulse);
            
            animator.SetBool("isStunned", true);    
            StartCoroutine(RecoverySequence(duration));
        }
        else
        {
            _rigidBody.isKinematic = true;
            _rigidBody.linearVelocity = Vector3.zero;

            animator.SetBool("isStunned", true);
            StartCoroutine(StaticStunSequence(duration));
        }
    }

    private IEnumerator StaticStunSequence(float duration)
    {
        yield return new WaitForSeconds(duration);

        if(currentState == State.Stunned && _rigidBody.isKinematic)
        {
            currentState = State.Chase;
            _agent.enabled = true;
        }
    }

    private IEnumerator RecoverySequence(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 4. ESPERAR A QUE TOQUE EL SUELO
        // Mientras la velocidad sea alta o no esté en el suelo, esperamos
        bool grounded = false;
        while (!grounded)
        {
            // Lanzamos un rayo pequeño hacia abajo para ver si estamos cerca del suelo
            grounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 0.8f);
            yield return new WaitForSeconds(0.1f);
        }

        // 5. VOLVER A PONERSE EN PIE
        _rigidBody.isKinematic = true; // Devolvemos el control al script
        _rigidBody.useGravity = false;
        
        animator.SetBool("isStunned", false); // Dispara tu animación de levantarse
        
        yield return new WaitForSeconds(1.0f); // Tiempo que tarda la animación de levantarse

        // 6. REACTIVAR IA
        _agent.enabled = true;
        currentState = State.Chase;
    }

    void UpdateAnimator()
    {
        if (animator != null)
            animator.SetFloat("Speed", _agent.velocity.magnitude);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Solo explotamos si estamos stuneados (en el aire por un golpe)
        if (currentState == State.Stunned)
        {
            // Calculamos la fuerza del impacto basada en la velocidad relativa
            float impactForce = collision.relativeVelocity.magnitude;

            if (impactForce > _collisionForceThreshold)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        // 1. Instanciar el efecto visual de explosión
        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }

        // 2. Opcional: Dañar a otros enemigos cercanos (Daño de área)
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider col in nearbyEnemies)
        {
            // Evitamos dañarnos a nosotros mismos antes de morir
            if (col.gameObject == this.gameObject) continue;

            if (col.TryGetComponent(out EnemyHealth health))
            {
                health.Damaged(_explosionDamage);
            }
        }

        // 3. Destruir al enemigo
        // Puedes llamar a una función de muerte o simplemente destruir el objeto
        Destroy(gameObject);
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